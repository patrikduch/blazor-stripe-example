namespace BlazorStripeExample.Controllers;

using BlazorStripeExample.Interfaces;
using BlazorStripeExample.Models.Common.Responses;
using BlazorStripeExample.Models.Stripe.Requests;
using BlazorStripeExample.Models.Stripe.Responses;
using Microsoft.AspNetCore.Mvc;
using System.IO;

[Route("api/stripe")]
[ApiController]
public class StripeController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IStripeService _stripeService;
    private readonly ISubscriptionService _subscriptionService;

    public StripeController(
        IConfiguration configuration,
        IStripeService stripeService,
        ISubscriptionService subscriptionService)
    {
        _configuration = configuration;
        _stripeService = stripeService;
        _subscriptionService = subscriptionService;
    }

    [HttpPost("create-checkout-session")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
    public async Task<IActionResult> CreateCheckoutSession([FromBody] CreateCheckoutSessionRequest dto)
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var result = await _stripeService.CreateCheckoutSessionAsync(dto.BillingInterval, baseUrl);

        if (result.Success)
            return Ok(new { result.SessionId });

        return BadRequest(new { error = result.Error });
    }


    [HttpGet("confirmation")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaymentConfirmationResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    public async Task<IActionResult> GetPaymentConfirmation([FromQuery] string sessionId)
    {
        var (found, data, error) = await _subscriptionService.GetPaymentConfirmationAsync(sessionId);

        if (!found)
        {
            if (error == "Session ID is required.")
                return BadRequest(new ErrorResponse { Error = error });

            return NotFound(new ErrorResponse { Error = error! });
        }

        return Ok(data);
    }

    [HttpPost("webhook")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
    public async Task<IActionResult> StripeWebhook()
    {
        var json = await new StreamReader(Request.Body).ReadToEndAsync();
        var signature = Request.Headers["Stripe-Signature"];
        var endpointSecret = _configuration["Stripe:WebhookSecret"];

        var (success, error) = await _stripeService.HandleWebhookAsync(json, signature, endpointSecret);

        return success
            ? Ok()
            : BadRequest(new ErrorResponse { Error = error ?? "Webhook processing failed." });
    }
}
