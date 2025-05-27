namespace BlazorStripeExample.Models.Requests;

public class CreateCheckoutSessionRequest
{
    public string BillingInterval { get; set; } = default!;
}