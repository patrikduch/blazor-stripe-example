namespace BlazorStripeExample.Controllers;

using Microsoft.AspNetCore.Mvc;
using Stripe;

[Route("api/[controller]")]
[ApiController]
public class StripeController : ControllerBase
{
    [HttpGet("test")]
    public IActionResult GetTest()
    {
        return Ok("Stripe test endpoint is working.");
    }

    [HttpPost("create-checkout-session")]
    public async Task<IActionResult> CreateCheckoutSession()
    {
        try
        {
            // ✅ Build base URL from request (e.g., https://localhost:7229)
            var request = HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";

            var options = new Stripe.Checkout.SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<Stripe.Checkout.SessionLineItemOptions>
                {
                    new Stripe.Checkout.SessionLineItemOptions
                    {
                        PriceData = new Stripe.Checkout.SessionLineItemPriceDataOptions
                        {
                            UnitAmount = 5000, // $50.00 in cents
                            Currency = "usd",
                            ProductData = new Stripe.Checkout.SessionLineItemPriceDataProductDataOptions
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

            var service = new Stripe.Checkout.SessionService();
            var session = await service.CreateAsync(options);

            return Ok(new { sessionId = session.Id });
        }
        catch (StripeException stripeEx)
        {
            Console.WriteLine($"Stripe error: {stripeEx.Message}");
            return StatusCode(500, new { error = "Stripe error occurred", detail = stripeEx.Message });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error: {ex.Message}");
            return StatusCode(500, new { error = "Internal server error", detail = ex.Message });
        }
    }
}
