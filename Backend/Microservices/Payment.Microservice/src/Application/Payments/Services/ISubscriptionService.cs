using System;
using System.Threading.Tasks;
using SharedLibrary.Contracts;

namespace Application.Payments.Services;

/// <summary>
/// Service for communicating with Subscription microservice via RabbitMQ
/// </summary>
public interface ISubscriptionService
{
    /// <summary>
    /// Get subscription price information by UserSubscriptionId
    /// </summary>
    Task<GetSubscriptionPriceResponse> GetSubscriptionPriceAsync(Guid userSubscriptionId);
}
