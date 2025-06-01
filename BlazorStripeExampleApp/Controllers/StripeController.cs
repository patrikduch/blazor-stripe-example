namespace BlazorStripeExample.Controllers;

using BlazorStripeExample.Contexts;
using BlazorStripeExample.Entities;
using BlazorStripeExample.Interfaces;
using BlazorStripeExample.Models.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    private readonly IStripeService _stripeService;
    private readonly AppDbContext _db;

    public StripeController(
        IConfiguration configuration,
        ILogger<StripeController> logger,
        IStripeService stripeService,
        AppDbContext db)
    {
        _configuration = configuration;
        _logger = logger;
        _db = db;
        _stripeService = stripeService;
    }

    [HttpPost("create-checkout-session")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(object))]
    public async Task<IActionResult> CreateCheckoutSession([FromBody] CreateCheckoutSessionRequest dto)
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var result = await _stripeService.CreateCheckoutSessionAsync(dto.BillingInterval, baseUrl);

        if (result.Success)
            return Ok(new { result.SessionId });

        return BadRequest(new { error = result.Error });
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
