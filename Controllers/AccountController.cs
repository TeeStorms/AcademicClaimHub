using ClaimManagementHub.Models;
using ClaimManagementHub.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ClaimManagementHub.Controllers
{
    public class AccountController : Controller
    {
        private readonly InMemoryUserService _userService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(InMemoryUserService userService, ILogger<AccountController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Dashboard", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password, string returnUrl = null)
        {
            try
            {
                var user = _userService.FindByUsername(email ?? string.Empty);
                if (user == null || !_userService.ValidatePassword(user, password ?? string.Empty))
                {
                    _logger.LogWarning("Failed login attempt for email: {Email}", email);

                    if (Request.Headers.XRequestedWith == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = "Invalid email or password" });
                    }

                    TempData["ErrorMessage"] = "Invalid email or password. Please try again.";
                    return View();
                }

                // Use fully qualified Claim type to avoid ambiguity
                var claims = new List<System.Security.Claims.Claim>
                {
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, user.Id),
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, user.FullName ?? string.Empty),
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, user.Email ?? string.Empty),
                    new System.Security.Claims.Claim("Role", user.Role ?? "User")
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8),
                    IssuedUtc = DateTimeOffset.UtcNow
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                _logger.LogInformation("User {Email} logged in successfully", user.Email);

                if (Request.Headers.XRequestedWith == "XMLHttpRequest")
                {
                    return Json(new
                    {
                        success = true,
                        redirectUrl = GetRedirectUrl(user.Role ?? "User"),
                        userName = user.FullName
                    });
                }

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction(GetRedirectAction(user.Role ?? "User"), GetRedirectController(user.Role ?? "User"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for email: {Email}", email);
                TempData["ErrorMessage"] = "An error occurred during login. Please try again.";
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            _logger.LogInformation("User logged out");

            if (Request.Headers.XRequestedWith == "XMLHttpRequest")
            {
                return Json(new { success = true });
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            _logger.LogWarning("Access denied for user {User} at {Path}", User.Identity?.Name, HttpContext.Request.Path);
            TempData["ErrorMessage"] = "You do not have permission to access this resource.";
            return View();
        }

        private static string GetRedirectUrl(string role)
        {
            return role switch
            {
                "Lecturer" => "/Lecturer/SubmitClaim",
                "Coordinator" or "Manager" => "/Coordinator/VerifyClaims",
                "HR" => "/HR/Dashboard",
                _ => "/Home/Dashboard"
            };
        }

        private static string GetRedirectAction(string role)
        {
            return role switch
            {
                "Lecturer" => "SubmitClaim",
                "Coordinator" or "Manager" => "VerifyClaims",
                "HR" => "Dashboard",
                _ => "Dashboard"
            };
        }

        private static string GetRedirectController(string role)
        {
            return role switch
            {
                "Lecturer" => "Lecturer",
                "Coordinator" or "Manager" => "Coordinator",
                "HR" => "HR",
                _ => "Home"
            };
        }
    }
}