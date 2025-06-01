namespace BlazorStripeExample.Services;

using BlazorStripeExample.Interfaces;
using BlazorStripeExample.Models.Settings;
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

    public async Task<string> CreateCheckoutSessionAsync(string billingInterval, string baseUrl)
    {
        var priceId = billingInterval.ToLower() switch
        {
            "monthly" => _stripeSettings.BasicMonthlyPriceId,
            "yearly" => _stripeSettings.BasicYearlyPriceId,
            _ => throw new ArgumentException("Invalid billing interval. Use 'monthly' or 'yearly'.")
        };

        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    Price = priceId,
                    Quantity = 1
                }
            },
            Mode = "subscription",
            SuccessUrl = $"{baseUrl}/success?session_id={{CHECKOUT_SESSION_ID}}",
            CancelUrl = $"{baseUrl}/cancel"
        };

        var service = new SessionService();
        var session = await service.CreateAsync(options);

        _logger.LogInformation("Checkout session created with ID: {SessionId}", session.Id);
        return session.Id;
    }
}
