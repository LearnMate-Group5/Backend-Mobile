using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class PaymentTransactionRepository : IPaymentTransactionRepository
    {
        private readonly MyDbContext _context;

        public PaymentTransactionRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task<PaymentTransaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.PaymentTransactions
                .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        }

        public async Task<PaymentTransaction?> GetByOrderIdAsync(string orderId, CancellationToken cancellationToken = default)
        {
            return await _context.PaymentTransactions
                .FirstOrDefaultAsync(t => t.OrderId == orderId, cancellationToken);
        }

        public async Task<PaymentTransaction?> GetByAppTransIdAsync(string appTransId, CancellationToken cancellationToken = default)
        {
            return await _context.PaymentTransactions
                .FirstOrDefaultAsync(t => t.AppTransId == appTransId, cancellationToken);
        }

        public async Task<IEnumerable<PaymentTransaction>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            return await _context.PaymentTransactions
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<PaymentTransaction>> GetUserPaymentHistoryAsync(
            string userId,
            int pageNumber = 1,
            int pageSize = 10,
            string? status = null,
            string? paymentGateway = null,
            CancellationToken cancellationToken = default)
        {
            var query = _context.PaymentTransactions
                .Where(t => t.UserId == userId);

            // Apply filters
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(t => t.Status == status);
            }

            if (!string.IsNullOrEmpty(paymentGateway))
            {
                query = query.Where(t => t.PaymentGateway == paymentGateway);
            }

            // Apply pagination
            return await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> GetUserPaymentHistoryCountAsync(
            string userId,
            string? status = null,
            string? paymentGateway = null,
            CancellationToken cancellationToken = default)
        {
            var query = _context.PaymentTransactions
                .Where(t => t.UserId == userId);

            // Apply filters
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(t => t.Status == status);
            }

            if (!string.IsNullOrEmpty(paymentGateway))
            {
                query = query.Where(t => t.PaymentGateway == paymentGateway);
            }

            return await query.CountAsync(cancellationToken);
        }

        public async Task AddAsync(PaymentTransaction transaction, CancellationToken cancellationToken = default)
        {
            await _context.PaymentTransactions.AddAsync(transaction, cancellationToken);
        }

        public async Task UpdateAsync(PaymentTransaction transaction, CancellationToken cancellationToken = default)
        {
            _context.PaymentTransactions.Update(transaction);
            await Task.CompletedTask;
        }
    }
}
