namespace BlazorStripeExample.Models.Stripe.Requests;

public class CreateCheckoutSessionRequest
{
    public string BillingInterval { get; set; } = default!;
}