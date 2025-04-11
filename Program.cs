using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using SmartInventory.Data;
using SmartInventory.Models;
using SmartInventory.Services;
using Serilog; // Add Serilog

// Configure Serilog for bootstrap logging (before host is built)
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
        .Build())
    .Enrich.FromLogContext()
    .CreateBootstrapLogger(); // Use CreateBootstrapLogger for early logging

try
{
    Log.Information("Starting web application");

    var builder = WebApplication.CreateBuilder(args);

    // --- Serilog Integration ---
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration) // Read from appsettings.json
        .ReadFrom.Services(services) // Allow enrichment from services
        .Enrich.FromLogContext()); // Add context information
    // --- End Serilog Integration ---


    // Configure services and EF Core with Npgsql using your connection string
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString)
           .LogTo(Console.WriteLine)); // Configure DbContext first

// Add Identity services
builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>() // Enable role management
    .AddEntityFrameworkStores<ApplicationDbContext>(); // Specify the EF Core store

// Configure SendGrid options from appsettings.json
builder.Services.Configure<SendGridOptions>(builder.Configuration.GetSection("SendGrid"));

// Register the SendGrid email sender (replace the dummy one)
builder.Services.AddTransient<IEmailSender, SendGridEmailSender>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Seed roles and potentially a default admin user
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Apply migrations first
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();

        // Then seed the roles
        await SeedData.InitializeAsync(services); // Call the seeding method
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred seeding the DB.");
        // Decide if you want to stop the application or continue
    }
}


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Add status code pages middleware to handle 404 etc.
app.UseStatusCodePagesWithReExecute("/Home/Error/{0}"); // {0} is the status code

// Add Authentication middleware BEFORE Authorization
app.UseAuthentication();
app.UseAuthorization();

// Map Razor Pages for Identity UI
app.MapRazorPages();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

    app.Run();

}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush(); // Ensure logs are flushed on shutdown
}
