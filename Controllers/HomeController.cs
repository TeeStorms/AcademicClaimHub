using ClaimManagementHub.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ClaimManagementHub.Controllers
{
    public class HomeController : Controller
    {
        private readonly IClaimsRepository _repo;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IClaimsRepository repo, ILogger<HomeController> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Dashboard");
            }
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var summary = await _repo.GetSummaryAsync();
                var recent = await _repo.GetRecentAsync(5);
                var userRole = User.FindFirst("Role")?.Value;

                var dashboardData = new
                {
                    Summary = summary,
                    RecentClaims = recent,
                    UserRole = userRole,
                    UserName = User.Identity?.Name,
                    UserAvatar = User.FindFirst("Avatar")?.Value ?? "/images/default-avatar.png"
                };

                ViewBag.DashboardData = dashboardData;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard data");
                TempData["ErrorMessage"] = "Unable to load dashboard data. Please try again.";
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetDashboardStats()
        {
            try
            {
                var summary = await _repo.GetSummaryAsync();
                return Json(new { success = true, data = summary });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching dashboard stats");
                return Json(new { success = false, message = "Error loading statistics" });
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var errorContext = HttpContext.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
            _logger.LogError(errorContext?.Error, "Application error occurred");

            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                ErrorMessage = TempData["ErrorMessage"]?.ToString()
            });
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Features()
        {
            return View();
        }
    }

    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public string? ErrorMessage { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}