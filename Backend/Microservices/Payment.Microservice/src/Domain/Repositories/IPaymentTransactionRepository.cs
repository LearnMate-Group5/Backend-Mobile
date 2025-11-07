using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;

namespace Domain.Repositories
{
    public interface IPaymentTransactionRepository
    {
        Task<PaymentTransaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<PaymentTransaction?> GetByOrderIdAsync(string orderId, CancellationToken cancellationToken = default);
        Task<PaymentTransaction?> GetByAppTransIdAsync(string appTransId, CancellationToken cancellationToken = default);
        Task<IEnumerable<PaymentTransaction>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<PaymentTransaction>> GetUserPaymentHistoryAsync(
            string userId,
            int pageNumber = 1,
            int pageSize = 10,
            string? status = null,
            string? paymentGateway = null,
            CancellationToken cancellationToken = default);
        Task<int> GetUserPaymentHistoryCountAsync(
            string userId,
            string? status = null,
            string? paymentGateway = null,
            CancellationToken cancellationToken = default);
        Task AddAsync(PaymentTransaction transaction, CancellationToken cancellationToken = default);
        Task UpdateAsync(PaymentTransaction transaction, CancellationToken cancellationToken = default);
    }
}
