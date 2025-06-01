namespace BlazorStripeExample.Interfaces;

using BlazorStripeExample.Models.Responses;
public interface IStripeService
{
    Task<CheckoutSessionResponse> CreateCheckoutSessionAsync(string interval, string baseUrl);
}
