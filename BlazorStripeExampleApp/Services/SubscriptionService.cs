namespace BlazorStripeExample.Services;

using BlazorStripeExample.Contexts;
using BlazorStripeExample.Interfaces;
using BlazorStripeExample.Models.Stripe.Responses;
using Microsoft.EntityFrameworkCore;

public class SubscriptionService : ISubscriptionService
{
    private readonly AppDbContext _db;
    private readonly ILogger<SubscriptionService> _logger;

    public SubscriptionService(AppDbContext db, ILogger<SubscriptionService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<(bool Found, PaymentConfirmationResponse? Data, string? Error)> GetPaymentConfirmationAsync(string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            return (false, null, "Session ID is required.");

        var payment = await _db.Payments.FirstOrDefaultAsync(p => p.SessionId == sessionId);

        if (payment == null)
        {
            _logger.LogWarning("❌ No payment found for Session ID: {SessionId}", sessionId);
            return (false, null, "Payment not found.");
        }

        _logger.LogInformation("✅ Payment found for Session ID: {SessionId}", sessionId);

        var response = new PaymentConfirmationResponse
        {
            SessionId = payment.SessionId,
            CustomerEmail = payment.CustomerEmail,
            Amount = payment.AmountTotal,
            Currency = payment.Currency.ToUpper()
        };

        return (true, response, null);
    }
}