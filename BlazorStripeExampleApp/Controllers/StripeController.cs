namespace BlazorStripeExample.Controllers;

using BlazorStripeExample.Contexts;
using BlazorStripeExample.Entities;
using BlazorStripeExample.Models.Requests;
using BlazorStripeExample.Models.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using System.IO;

[Route("api/[controller]")]
[ApiController]
public class StripeController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<StripeController> _logger;
    private readonly StripeSettings _stripeSettings;
    private readonly AppDbContext _db;

    public StripeController(IConfiguration configuration, ILogger<StripeController> logger, IOptions<StripeSettings> stripeSettings, AppDbContext db)
    {
        _configuration = configuration;
        _logger = logger;
        _db = db;
        _stripeSettings = stripeSettings.Value;
    }

    [HttpPost("create-checkout-session")]
    public async Task<IActionResult> CreateCheckoutSession([FromBody] CreateCheckoutSessionRequest requestDto)
    {
        try
        {
            var priceId = requestDto.BillingInterval.ToLower() switch
            {
                "monthly" => _stripeSettings.BasicMonthlyPriceId,
                "yearly" => _stripeSettings.BasicYearlyPriceId,
                _ => throw new ArgumentException("Invalid billing interval. Use 'monthly' or 'yearly'.")
            };

            var request = HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";

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
            return Ok(new { sessionId = session.Id });
        }
        catch (StripeException stripeEx)
        {
            _logger.LogError(stripeEx, "Stripe error occurred while creating session.");
            return StatusCode(500, new { error = "Stripe error occurred", detail = stripeEx.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred.");
            return StatusCode(500, new { error = "Internal server error", detail = ex.Message });
        }
    }

    [HttpGet("confirmation")]
    public async Task<IActionResult> GetPaymentConfirmation([FromQuery] string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            return BadRequest(new { message = "Session ID is required." });
        }

        var payment = await _db.Payments.FirstOrDefaultAsync(p => p.SessionId == sessionId);

        if (payment == null)
        {
            _logger.LogWarning("❌ No payment found for Session ID: {SessionId}", sessionId);
            return NotFound(new { message = "Payment not found." });
        }

        _logger.LogInformation("✅ Payment found for Session ID: {SessionId}", sessionId);

        return Ok(new
        {
            payment.SessionId,
            payment.CustomerEmail,
            Amount = payment.AmountTotal,
            Currency = payment.Currency.ToUpper()
        });
    }


    [HttpPost("webhook")]
    public async Task<IActionResult> StripeWebhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var endpointSecret = _configuration["Stripe:WebhookSecret"];

        try
        {
            var stripeEvent = EventUtility.ConstructEvent(
                json,
                Request.Headers["Stripe-Signature"],
                endpointSecret
            );

            _logger.LogInformation("Received Stripe event of type: {EventType}", stripeEvent.Type);

 
            if (stripeEvent.Type == EventTypes.CheckoutSessionCompleted)
            {
                var session = stripeEvent.Data.Object as Session;

                if (session != null)
                {
                    _logger.LogInformation("✅ Checkout session completed: {SessionId}", session.Id);

                    long amount = (long)((session.AmountTotal ?? 0) / 100m);

                    var payment = new Payment
                    {
                        SessionId = session.Id,
                        CustomerEmail = session.CustomerDetails?.Email ?? "unknown",
                        AmountTotal = amount,
                        Currency = session.Currency ?? "usd"
                    };

                    _db.Payments.Add(payment);
                    await _db.SaveChangesAsync();
                }
            }

            return Ok();
        }
        catch (StripeException e)
        {
            _logger.LogWarning(e, "Stripe exception during webhook handling.");
            return BadRequest();
        }
    }

}
