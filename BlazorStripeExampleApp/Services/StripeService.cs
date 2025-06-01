namespace BlazorStripeExample.Services;

using BlazorStripeExample.Contexts;
using BlazorStripeExample.Entities;
using BlazorStripeExample.Interfaces;
using BlazorStripeExample.Models.Settings;
using BlazorStripeExample.Models.Stripe.Responses;
using Stripe;
using Stripe.Checkout;

public class StripeService : IStripeService
{
    private readonly AppDbContext _db;
    private readonly StripeSettings _stripeSettings;
    private readonly ILogger<StripeService> _logger;

    public StripeService(AppDbContext db, StripeSettings stripeSettings, ILogger<StripeService> logger)
    {
        _db = db;
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

    public async Task<(bool Success, string? Error)> HandleWebhookAsync(string json, string signature, string endpointSecret)
    {
        try
        {
            var stripeEvent = EventUtility.ConstructEvent(json, signature, endpointSecret);
            _logger.LogInformation("Received Stripe event: {EventType}", stripeEvent.Type);

            if (stripeEvent.Type == EventTypes.CheckoutSessionCompleted &&
                stripeEvent.Data.Object is Session session)
            {
                _logger.LogInformation("✅ Checkout session completed: {SessionId}", session.Id);

                var payment = new Payment
                {
                    SessionId = session.Id,
                    CustomerEmail = session.CustomerDetails?.Email ?? "unknown",
                    AmountTotal = (long)((session.AmountTotal ?? 0) / 100m),
                    Currency = session.Currency ?? "usd"
                };

                _db.Payments.Add(payment);
                await _db.SaveChangesAsync();
            }

            return (true, null);
        }
        catch (StripeException ex)
        {
            _logger.LogWarning(ex, "Stripe error during webhook");
            return (false, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during webhook");
            return (false, "Internal server error");
        }
    }


}
