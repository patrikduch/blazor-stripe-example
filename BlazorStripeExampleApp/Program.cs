using BlazorStripeExample.Components;
using BlazorStripeExample.Models.Settings;
using Stripe;

namespace BlazorStripeExample;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add Stripe configuration from appsettings
        builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));


        // Add services to the container.

        builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents(); // ✅ Register interactive server components

        // Combination Blazor + Web API
        builder.Services.AddControllers();

        builder.Services.AddHttpClient();

        var app = builder.Build();

        // Set Stripe API key
        var stripeSettings = app.Services.GetRequiredService<IConfiguration>().GetSection("Stripe").Get<StripeSettings>();
        StripeConfiguration.ApiKey = stripeSettings.SecretKey;

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseStaticFiles();
        app.UseAntiforgery();

        // 🔥 Add interactive render mode for components
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();
        app.MapControllers(); // Applying Blazor App + API

        app.Run();
    }
}
