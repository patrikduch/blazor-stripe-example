using BlazorStripeExample.Components;
using BlazorStripeExample.Contexts;
using BlazorStripeExample.Models.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.WebHost.ConfigureKestrel(options =>
        {
            options.ListenAnyIP(5000); // HTTP
            options.ListenAnyIP(7229, listenOptions => listenOptions.UseHttps()); // HTTPS
        });

        builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


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

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "BlazorStripeExample API v1");
            });
        }


        app.MapGet("/health", async ([FromServices] AppDbContext db) =>
        {
            try
            {
                var canConnect = await db.Database.CanConnectAsync();
                return canConnect
                    ? Results.Ok("Database connection is healthy ✅")
                    : Results.Problem("Database connection failed ❌");
            }
            catch (Exception ex)
            {
                return Results.Problem($"Database health check threw an exception ❌: {ex.Message}");
            }
        });

        app.Run();
    }
}
