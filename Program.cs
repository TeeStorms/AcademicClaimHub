using ClaimManagementHub.Hubs;
using ClaimManagementHub.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

// Add MVC services with enhanced configuration
builder.Services.AddControllersWithViews()
    .AddRazorOptions(options =>
    {
        options.PageViewLocationFormats.Add("/Views/Shared/{0}.cshtml");
    });

// Add SignalR for real-time features
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.MaximumReceiveMessageSize = 1024 * 1024; // 1MB
});

// Add services to the container with modern configuration
builder.Services.AddSingleton<IClaimsRepository, InMemoryClaimsRepository>();
builder.Services.AddSingleton<InMemoryUserService>();
builder.Services.AddSingleton<ClaimValidator>();
builder.Services.AddSingleton<ApprovalWorkflowService>();

// Add the new FileUploadService
builder.Services.AddScoped<FileUploadService>();

// Add HTTP context accessor for modern user management
builder.Services.AddHttpContextAccessor();

// Modern authorization builder with enhanced policies
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("LecturerOnly", policy =>
        policy.RequireClaim("Role", "Lecturer")
              .RequireAuthenticatedUser())
    .AddPolicy("CoordinatorOnly", policy =>
        policy.RequireClaim("Role", "Coordinator", "Manager")
              .RequireAuthenticatedUser())
    .AddPolicy("HROnly", policy =>
        policy.RequireClaim("Role", "HR")
              .RequireAuthenticatedUser())
    .AddPolicy("ManagerOnly", policy =>
        policy.RequireClaim("Role", "Manager")
              .RequireAuthenticatedUser());

// Enhanced authentication configuration
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8); // Longer sessions
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
    });

// Enhanced file upload configuration
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10 MB
    options.MemoryBufferThreshold = 1024 * 1024; // 1 MB
    options.ValueLengthLimit = int.MaxValue;
});

// Add response compression for better performance
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

// Add logging with modern configuration
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.AddDebug();
    logging.SetMinimumLevel(LogLevel.Information);
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts(); // HTTP Strict Transport Security
}
else
{
    // Detailed errors in development
    app.UseDeveloperExceptionPage();
}

// Enable response compression
app.UseResponseCompression();

app.UseHttpsRedirection();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // Cache static files for better performance
        ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=31536000");
    }
});

app.UseRouting();

// Enhanced security headers
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");

    // Add CSP for better security
    context.Response.Headers.Append("Content-Security-Policy",
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline' https://cdn.tailwindcss.com https://cdnjs.cloudflare.com; " +
        "style-src 'self' 'unsafe-inline' https://cdn.tailwindcss.com https://fonts.googleapis.com https://cdnjs.cloudflare.com; " +
        "font-src 'self' https://fonts.gstatic.com; " +
        "connect-src 'self' wss://*;"); // Allow WebSocket connections for SignalR

    await next();
});

app.UseAuthentication();
app.UseAuthorization();

// MVC routing with enhanced pattern
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Additional routes for better organization
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

// SignalR hub mapping for real-time features
app.MapHub<ClaimHub>("/claimHub"); // Changed from ClaimManagementHub to ClaimHub

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "Healthy", timestamp = DateTime.UtcNow }));

app.Run();