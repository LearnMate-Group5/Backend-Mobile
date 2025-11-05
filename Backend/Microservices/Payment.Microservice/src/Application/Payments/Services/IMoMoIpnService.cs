using System.Threading.Tasks;
using Application.Payments.DTOs;

namespace Application.Payments.Services
{
    public interface IMoMoIpnService
    {
        /// <summary>
        /// Process IPN callback from MoMo
        /// </summary>
        Task<MoMoIpnResponse> ProcessIpnAsync(MoMoIpnRequest ipnRequest);
        
        /// <summary>
        /// Verify the signature of IPN request from MoMo
        /// </summary>
        bool VerifySignature(MoMoIpnRequest ipnRequest);
    }
}
