namespace BlazorStripeExample.Models.Stripe.Responses;

public class CheckoutSessionResponse
{
    public bool Success { get; init; }
    public string? SessionId { get; init; }
    public string? Error { get; init; }
}
