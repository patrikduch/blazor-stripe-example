namespace BlazorStripeExample.Controllers;

using BlazorStripeExample.Contexts;
using BlazorStripeExample.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Stripe;
using Stripe.Checkout;
using System.IO;

[Route("api/[controller]")]
[ApiController]
public class StripeController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<StripeController> _logger;

    private readonly AppDbContext _db;

    public StripeController(IConfiguration configuration, ILogger<StripeController> logger, AppDbContext db)
    {
        _configuration = configuration;
        _logger = logger;
        _db = db;
    }

    [HttpGet("test")]
    public IActionResult GetTest()
    {
        _logger.LogInformation("Stripe test endpoint hit.");
        return Ok("Stripe test endpoint is working.");
    }

    [HttpPost("create-checkout-session")]
    public async Task<IActionResult> CreateCheckoutSession()
    {
        try
        {
            var request = HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = 5000,
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "Test Product",
                            },
                        },
                        Quantity = 1,
                    },
                },
                Mode = "payment",
                SuccessUrl = $"{baseUrl}/success",
                CancelUrl = $"{baseUrl}/cancel",
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

            if (stripeEvent.Type == "checkout.session.completed")
            {
                var session = stripeEvent.Data.Object as Session;
                _logger.LogInformation("💰 Payment successful for session: {SessionId}", session?.Id);

                if (session != null)
                {
                    var payment = new Payment
                    {
                        SessionId = session.Id,
                        CustomerEmail = session.CustomerDetails?.Email ?? "unknown",
                        AmountTotal = session.AmountTotal ?? 0,
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
