using System.Threading.Tasks;
using Application.Payments.DTOs;

namespace Application.Payments.Services
{
    public interface IMoMoService
    {
        Task<CreateMoMoOrderResponse> CreateOrderAsync(string userId, CreateMoMoOrderRequest request);
    }
}
