namespace BlazorStripeExample.Entities;

public class Payment
{
    public int Id { get; set; }
    public string SessionId { get; set; } = null!;
    public string CustomerEmail { get; set; } = null!;
    public decimal AmountTotal { get; set; }
    public string Currency { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
