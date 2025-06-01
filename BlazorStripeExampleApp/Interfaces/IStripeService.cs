namespace BlazorStripeExample.Interfaces;

using BlazorStripeExample.Models.Stripe.Responses;

public interface IStripeService
{
    Task<CheckoutSessionResponse> CreateCheckoutSessionAsync(string interval, string baseUrl);

    Task<(bool Success, string? Error)> HandleWebhookAsync(string json, string signature);
}
