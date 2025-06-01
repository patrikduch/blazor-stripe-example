namespace BlazorStripeExample.Services;

using BlazorStripeExample.Interfaces;
using BlazorStripeExample.Models.Responses;
using BlazorStripeExample.Models.Settings;
using Stripe;
using Stripe.Checkout;

public class StripeService : IStripeService
{
    private readonly StripeSettings _stripeSettings;
    private readonly ILogger<StripeService> _logger;

    public StripeService(StripeSettings stripeSettings, ILogger<StripeService> logger)
    {
        _stripeSettings = stripeSettings;
        _logger = logger;
    }

    public async Task<CheckoutSessionResponse> CreateCheckoutSessionAsync(string interval, string baseUrl)
    {
        try
        {
            var priceId = interval.ToLower() switch
            {
                "monthly" => _stripeSettings.BasicMonthlyPriceId,
                "yearly" => _stripeSettings.BasicYearlyPriceId,
                _ => throw new ArgumentException("Invalid billing interval.")
            };

            var session = await new SessionService().CreateAsync(new SessionCreateOptions
            {
                PaymentMethodTypes = new() { "card" },
                LineItems = new() { new() { Price = priceId, Quantity = 1 } },
                Mode = "subscription",
                SuccessUrl = $"{baseUrl}/success?session_id={{CHECKOUT_SESSION_ID}}",
                CancelUrl = $"{baseUrl}/cancel"
            });

            return new CheckoutSessionResponse { Success = true, SessionId = session.Id };
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error");
            return new CheckoutSessionResponse { Success = false, Error = ex.Message };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error");
            return new CheckoutSessionResponse { Success = false, Error = "Internal error: " + ex.Message };
        }
    }


}
