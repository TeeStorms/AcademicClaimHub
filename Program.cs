using ClaimManagementHub.Services;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddSingleton<IClaimsRepository, InMemoryClaimsRepository>();
builder.Services.AddSingleton<InMemoryUserService>();

// 👉 Add FluentValidation + Workflow services
builder.Services.AddSingleton<ClaimValidator>();
builder.Services.AddSingleton<ApprovalWorkflowService>();

// Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
    });

// Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("LecturerOnly", policy => policy.RequireClaim("Role", "Lecturer"));
    options.AddPolicy("CoordinatorOnly", policy => policy.RequireClaim("Role", "Coordinator", "Manager"));
    options.AddPolicy("HROnly", policy => policy.RequireClaim("Role", "HR"));
    options.AddPolicy("ManagerOnly", policy => policy.RequireClaim("Role", "Manager"));
});

// File upload limits
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10 MB
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
