using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Payments.DTOs;
using Domain.Repositories;
using SharedLibrary.Abstractions.Messaging;
using SharedLibrary.Common.ResponseModel;

namespace Application.Payments.Queries
{
    internal sealed class GetPaymentHistoryQueryHandler
        : IQueryHandler<GetPaymentHistoryQuery, PaymentHistoryPaginatedResponse>
    {
        private readonly IPaymentTransactionRepository _paymentTransactionRepository;

        public GetPaymentHistoryQueryHandler(IPaymentTransactionRepository paymentTransactionRepository)
        {
            _paymentTransactionRepository = paymentTransactionRepository;
        }

        public async Task<Result<PaymentHistoryPaginatedResponse>> Handle(
            GetPaymentHistoryQuery query,
            CancellationToken cancellationToken)
        {
            // Validate page number and page size
            if (query.PageNumber < 1)
            {
                return Result.Failure<PaymentHistoryPaginatedResponse>(
                    new Error("Payment.InvalidPageNumber", "Page number must be greater than 0"));
            }

            if (query.PageSize < 1 || query.PageSize > 100)
            {
                return Result.Failure<PaymentHistoryPaginatedResponse>(
                    new Error("Payment.InvalidPageSize", "Page size must be between 1 and 100"));
            }

            // Get total count
            var totalCount = await _paymentTransactionRepository.GetUserPaymentHistoryCountAsync(
                query.UserId,
                query.Status,
                query.PaymentGateway,
                cancellationToken);

            // Get paginated transactions
            var transactions = await _paymentTransactionRepository.GetUserPaymentHistoryAsync(
                query.UserId,
                query.PageNumber,
                query.PageSize,
                query.Status,
                query.PaymentGateway,
                cancellationToken);

            // Map to response DTOs
            var items = transactions.Select(t => new PaymentHistoryResponse
            {
                Id = t.Id,
                OrderId = t.OrderId,
                Amount = t.Amount,
                PaymentGateway = t.PaymentGateway,
                Status = t.Status,
                OrderInfo = t.OrderInfo,
                Message = t.Message,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt,
                ExpiresAt = t.ExpiresAt,
                TransactionId = t.PaymentGateway == "ZaloPay" ? t.AppTransId : t.MomoTransId,
                PaymentUrl = t.PaymentGateway == "MoMo" ? t.PayUrl : null
            }).ToList();

            // Calculate pagination metadata
            var totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);

            var response = new PaymentHistoryPaginatedResponse
            {
                Items = items,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
                HasPreviousPage = query.PageNumber > 1,
                HasNextPage = query.PageNumber < totalPages
            };

            return Result.Success(response);
        }
    }
}
