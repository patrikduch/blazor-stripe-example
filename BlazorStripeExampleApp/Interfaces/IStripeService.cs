namespace BlazorStripeExample.Interfaces;

public interface IStripeService
{
    Task<string> CreateCheckoutSessionAsync(string billingInterval, string baseUrl);
}
