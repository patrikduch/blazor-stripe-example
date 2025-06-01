namespace BlazorStripeExample.Interfaces;

using BlazorStripeExample.Models.Stripe.Responses;

public interface ISubscriptionService
{
    Task<(bool Found, PaymentConfirmationResponse? Data, string? Error)> GetPaymentConfirmationAsync(string sessionId);
}
