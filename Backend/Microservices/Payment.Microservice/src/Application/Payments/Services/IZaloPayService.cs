using System.Threading.Tasks;
using Application.Payments.DTOs;

namespace Application.Payments.Services
{
    public interface IZaloPayService
    {
        Task<CreateZaloPayOrderResponse> CreateOrderAsync(string userId, CreateZaloPayOrderRequest request);
        Task<ZaloPayQueryResponse> QueryOrderAsync(string appTransId);
    }
}
