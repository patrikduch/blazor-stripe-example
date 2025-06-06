﻿namespace BlazorStripeExample.Models.Settings;

public class StripeSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public string PublishableKey { get; set; } = string.Empty;

    public string WebhookSecret { get; set; } = string.Empty;

    public string BasicMonthlyPriceId { get; set; } = string.Empty;
    public string BasicYearlyPriceId { get; set; } = string.Empty;
}