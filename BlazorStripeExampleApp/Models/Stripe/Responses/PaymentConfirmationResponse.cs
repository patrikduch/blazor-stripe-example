namespace BlazorStripeExample.Models.Stripe.Responses;

public class PaymentConfirmationResponse
{
    public string SessionId { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
}
