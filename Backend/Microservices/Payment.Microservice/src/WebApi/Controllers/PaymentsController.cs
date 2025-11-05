using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Application.Payments.Commands;
using Application.Payments.DTOs;
using Application.Payments.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SharedLibrary.Attributes;
using SharedLibrary.Common;
using SharedLibrary.Common.Commands;
using SharedLibrary.Common.ResponseModel;
using AllowAnonymous = Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute;

namespace WebApi.Controllers
{
    /// <summary>
    /// Payment operations controller
    /// </summary>
    [ApiController]
    [Route("api/payment")]
    public class PaymentsController : ApiController
    {
        private readonly IZaloPayIpnService _zaloPayIpnService;
        private readonly IMoMoIpnService _momoIpnService;
        private readonly ILogger<PaymentsController> _logger;
        private readonly IZaloPayService _zaloPayService;
        private readonly IMoMoService _momoService;

        /// <summary>
        /// Initialize payment controller
        /// </summary>
        public PaymentsController(
            IMediator mediator,
            IZaloPayIpnService zaloPayIpnService,
            IMoMoIpnService momoIpnService,
            ILogger<PaymentsController> logger,
            IZaloPayService zaloPayService,
            IMoMoService momoService) : base(mediator)
        {
            _zaloPayIpnService = zaloPayIpnService;
            _momoIpnService = momoIpnService;
            _logger = logger;
            _zaloPayService = zaloPayService;
            _momoService = momoService;
        }

        /// <summary>
        /// Create a ZaloPay order for payment
        /// </summary>
        /// <param name="request">Order creation request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Order information including payment URL and QR code</returns>
        [HttpPost("zalopay/create")]
        [Authorize("User", "Admin")]
        public async Task<IActionResult> CreateZaloPayOrder(
            [FromBody] CreateZaloPayOrderRequest request,
            CancellationToken cancellationToken)
        {
            // Get the logged-in user ID from JWT claims
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? User.FindFirst("sub")?.Value
                         ?? User.FindFirst("userId")?.Value;

            if (string.IsNullOrWhiteSpace(userId))
            {
                return BadRequest(new { message = "User ID not found in authentication token" });
            }

            var command = new CreateZaloPayOrderCommand(
                UserId: userId,
                OrderId: request.OrderId,
                Description: request.Description,
                RedirectUrl: request.RedirectUrl
            );

            var result = await _mediator.Send(command, cancellationToken);
            if (result.IsFailure)
            {
                return HandleFailure(result);
            }

            var commit = await _mediator.Send(new SaveChangesCommand(), cancellationToken);
            if (commit.IsFailure)
            {
                return HandleFailure(commit);
            }

            return Ok(result.Value);
        }

        /// <summary>
        /// Create a MoMo order for payment
        /// </summary>
        /// <param name="request">Order creation request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Order information including payment URL and deeplink</returns>
        [HttpPost("momo/create")]
        [Authorize("User", "Admin")]
        public async Task<IActionResult> CreateMoMoOrder(
            [FromBody] CreateMoMoOrderRequest request,
            CancellationToken cancellationToken)
        {
            // Get the logged-in user ID from JWT claims
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? User.FindFirst("sub")?.Value
                         ?? User.FindFirst("userId")?.Value;

            if (string.IsNullOrWhiteSpace(userId))
            {
                return BadRequest(new { message = "User ID not found in authentication token" });
            }

            var command = new CreateMoMoOrderCommand(
                UserId: userId,
                OrderId: request.OrderId,
                OrderInfo: request.OrderInfo,
                RedirectUrl: request.RedirectUrl,
                ExtraData: request.ExtraData,
                Lang: request.Lang
            );

            var result = await _mediator.Send(command, cancellationToken);
            if (result.IsFailure)
            {
                return HandleFailure(result);
            }

            var commit = await _mediator.Send(new SaveChangesCommand(), cancellationToken);
            if (commit.IsFailure)
            {
                return HandleFailure(commit);
            }

            return Ok(result.Value);
        }

        /// <summary>
        /// ZaloPay IPN (Instant Payment Notification) callback endpoint
        /// This endpoint receives payment notifications from ZaloPay servers
        /// </summary>
        /// <param name="ipnRequest">Payment notification from ZaloPay</param>
        /// <returns>Acknowledgment response to ZaloPay</returns>
        [HttpPost("zalopay/ipn")]
        [AllowAnonymousAttribute]
        public async Task<IActionResult> ZaloPayIpnCallback([FromBody] ZaloPayIpnRequest ipnRequest)
        {
            try
            {
                _logger.LogInformation("Received ZaloPay IPN callback. Type: {Type}", ipnRequest.Type);

                var response = await _zaloPayIpnService.ProcessIpnAsync(ipnRequest);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing ZaloPay IPN callback");

                // Return error response to ZaloPay
                return Ok(new ZaloPayIpnResponse
                {
                    ReturnCode = 0,
                    ReturnMessage = "System error"
                });
            }
        }

        /// <summary>
        /// MoMo IPN (Instant Payment Notification) callback endpoint
        /// This endpoint receives payment notifications from MoMo servers
        /// </summary>
        /// <param name="ipnRequest">Payment notification from MoMo</param>
        /// <returns>Acknowledgment response to MoMo</returns>
        [HttpPost("momo/ipn")]
        [AllowAnonymousAttribute]
        public async Task<IActionResult> MoMoIpnCallback([FromBody] MoMoIpnRequest ipnRequest)
        {
            try
            {
                _logger.LogInformation("Received MoMo IPN callback for OrderId: {OrderId}, ResultCode: {ResultCode}",
                    ipnRequest.OrderId, ipnRequest.ResultCode);

                var response = await _momoIpnService.ProcessIpnAsync(ipnRequest);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing MoMo IPN callback for OrderId: {OrderId}", ipnRequest.OrderId);

                // Return error response to MoMo
                return Ok(new MoMoIpnResponse
                {
                    PartnerCode = ipnRequest.PartnerCode,
                    OrderId = ipnRequest.OrderId,
                    RequestId = ipnRequest.RequestId,
                    ResultCode = 99,
                    Message = "System error",
                    ResponseTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    ExtraData = "",
                    Signature = ""
                });
            }
        }

        /// <summary>
        /// Query ZaloPay order status and update transaction if successful
        /// </summary>
        /// <param name="appTransId">ZaloPay transaction ID</param>
        /// <returns>Query result from ZaloPay</returns>
        [HttpGet("zalopay/query/{appTransId}")]
        [Authorize("User", "Admin")]
        public async Task<IActionResult> QueryZaloPayOrder(string appTransId)
        {
            try
            {
                _logger.LogInformation("Querying ZaloPay order status for AppTransId: {AppTransId}", appTransId);

                var result = await _zaloPayService.QueryOrderAsync(appTransId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying ZaloPay order for AppTransId: {AppTransId}", appTransId);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Health check endpoint
        /// </summary>
        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new { status = "ok", service = "payment" });
        }
    }
}
