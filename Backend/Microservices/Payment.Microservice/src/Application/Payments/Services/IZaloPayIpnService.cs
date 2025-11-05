using System.Threading.Tasks;
using Application.Payments.DTOs;

namespace Application.Payments.Services
{
    /// <summary>
    /// Service for handling ZaloPay IPN (Instant Payment Notification) callbacks
    /// </summary>
    public interface IZaloPayIpnService
    {
        /// <summary>
        /// Process ZaloPay IPN callback and update payment transaction
        /// </summary>
        /// <param name="ipnRequest">IPN request from ZaloPay</param>
        /// <returns>Response to send back to ZaloPay</returns>
        Task<ZaloPayIpnResponse> ProcessIpnAsync(ZaloPayIpnRequest ipnRequest);

        /// <summary>
        /// Verify MAC signature from ZaloPay IPN
        /// </summary>
        /// <param name="data">Data string from IPN</param>
        /// <param name="mac">MAC signature from IPN</param>
        /// <returns>True if signature is valid</returns>
        bool VerifyMac(string data, string mac);
    }
}
