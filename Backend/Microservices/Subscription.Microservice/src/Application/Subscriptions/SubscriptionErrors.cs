using System;
using SharedLibrary.Common.ResponseModel;

namespace Application.Subscriptions;

public static class SubscriptionErrors
{
    public static Error NotFound(Guid subscriptionId) =>
        new("Subscription.NotFound", $"Subscription plan with id '{subscriptionId}' was not found.");

    public static Error DuplicateName(string name) =>
        new("Subscription.DuplicateName", $"A subscription plan with name '{name}' already exists.");

    public static Error DuplicateNameAndType(string name, string type) =>
        new("Subscription.DuplicateNameAndType", $"A subscription plan with name '{name}' and type '{type}' already exists.");

    public static Error DuplicateType(string type) =>
        new("Subscription.DuplicateType", $"A subscription plan with type '{type}' already exists.");

    public static Error InactiveSubscription(Guid subscriptionId) =>
        new("Subscription.Inactive", $"Subscription plan with id '{subscriptionId}' is not active.");

    public static Error UserAlreadyHasSubscription(string userId) =>
        new("UserSubscription.AlreadyExists", $"User '{userId}' already has an active subscription.");

    public static Error UserSubscriptionNotFound(string userId) =>
        new("UserSubscription.NotFound", $"User '{userId}' does not have an active subscription to update.");

    public static Error UpgradeRequiresHigherPrice(Guid subscriptionId) =>
        new("Subscription.UpgradeRequiresHigherPrice", $"Subscription plan '{subscriptionId}' must have a higher price than the current plan.");

    public static Error SubscriptionAlreadyCurrent(Guid subscriptionId) =>
        new("Subscription.AlreadyCurrent", $"Subscription plan '{subscriptionId}' is already the current plan.");

    public static Error CurrentSubscriptionInactive(string userId) =>
        new("UserSubscription.Inactive", $"User '{userId}' does not have an active subscription to update.");

    public static Error SubscriptionAlreadyCancelled(string userId) =>
        new("UserSubscription.AlreadyCancelled", $"User '{userId}' subscription is already cancelled.");
}
