using Application.Payments.DTOs;
using SharedLibrary.Abstractions.Messaging;

namespace Application.Payments.Queries
{
    public sealed record GetPaymentHistoryQuery(
        string UserId,
        int PageNumber = 1,
        int PageSize = 10,
        string? Status = null,
        string? PaymentGateway = null
    ) : IQuery<PaymentHistoryPaginatedResponse>;
}
