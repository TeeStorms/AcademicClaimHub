using ClaimManagementHub.Models;
using ClaimManagementHub.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ClaimManagementHub.Controllers
{
    [Authorize(Policy = "HROnly")]
    public class HRController : Controller
    {
        private readonly IClaimsRepository _claimsRepository;
        private readonly InMemoryUserService _userService;
        private readonly ILogger<HRController> _logger;

        public HRController(IClaimsRepository claimsRepository, InMemoryUserService userService, ILogger<HRController> logger)
        {
            _claimsRepository = claimsRepository;
            _userService = userService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var claims = await _claimsRepository.GetAllAsync();
                var users = _userService.GetAllUsers();
                var lecturers = users.Where(u => u.Role == "Lecturer").ToList();
                var summary = await _claimsRepository.GetSummaryAsync();
                var workflowAnalysis = await _claimsRepository.GetWorkflowAnalysisAsync();

                // Build lecturer summaries
                var lecturerSummaries = lecturers.Select(lecturer => new
                {
                    FullName = lecturer.FullName,
                    Email = lecturer.Email,
                    TotalClaims = claims.Count(c => c.LecturerName == lecturer.FullName),
                    PendingClaims = claims.Count(c => c.LecturerName == lecturer.FullName && c.Status == "pending"),
                    ApprovedClaims = claims.Count(c => c.LecturerName == lecturer.FullName && (c.Status == "approved" || c.Status == "auto-approved")),
                    TotalApproved = claims.Where(c => c.LecturerName == lecturer.FullName && (c.Status == "approved" || c.Status == "auto-approved")).Sum(c => c.TotalAmount),
                    LastSubmission = claims.Where(c => c.LecturerName == lecturer.FullName).OrderByDescending(c => c.SubmittedAt).FirstOrDefault()?.SubmittedAt
                }).ToList();

                // Build payment summary
                var approvedClaims = claims.Where(c => c.Status == "approved" || c.Status == "auto-approved").ToList();
                var paymentSummary = new
                {
                    ReadyForPayment = approvedClaims.Count,
                    TotalAmount = approvedClaims.Sum(c => c.TotalAmount),
                    TotalLecturers = approvedClaims.Select(c => c.LecturerName).Distinct().Count(),
                    ProcessedThisMonth = approvedClaims.Count(c => c.ReviewedAt.HasValue && c.ReviewedAt.Value.Month == DateTime.UtcNow.Month),
                    AverageClaimAmount = approvedClaims.Count > 0 ? approvedClaims.Average(c => c.TotalAmount) : 0
                };

                ViewBag.Lecturers = lecturerSummaries;
                ViewBag.PaymentSummary = paymentSummary;
                ViewBag.ClaimsSummary = summary;
                ViewBag.WorkflowAnalysis = workflowAnalysis;
                ViewBag.TotalLecturers = lecturers.Count;

                // Report period defaults
                ViewBag.ReportFromDate = DateTime.UtcNow.AddMonths(-1).ToString("yyyy-MM-dd");
                ViewBag.ReportToDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading HR dashboard");
                TempData["ErrorMessage"] = "Unable to load dashboard data. Please try again.";
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> GenerateReport(string reportType, DateTime fromDate, DateTime toDate)
        {
            try
            {
                var claims = await _claimsRepository.GetAllAsync();
                var filteredClaims = claims.Where(c => c.SubmittedAt >= fromDate && c.SubmittedAt <= toDate.AddDays(1).AddSeconds(-1));

                object report = reportType switch
                {
                    "monthly" => new
                    {
                        Title = $"Monthly Claims Report: {fromDate:dd MMM yyyy} to {toDate:dd MMM yyyy}",
                        TotalClaims = filteredClaims.Count(),
                        PendingClaims = filteredClaims.Count(c => c.Status == "pending"),
                        ApprovedClaims = filteredClaims.Count(c => c.Status == "approved" || c.Status == "auto-approved"),
                        RejectedClaims = filteredClaims.Count(c => c.Status == "rejected"),
                        TotalAmount = filteredClaims.Where(c => c.Status == "approved" || c.Status == "auto-approved").Sum(c => c.TotalAmount),
                        // In the GenerateReport method, fix the AverageProcessingTime calculation:
                        AverageProcessingTime = filteredClaims.Where(c => c.ReviewedAt.HasValue)
                        .Average(c => (c.ReviewedAt!.Value - c.SubmittedAt).TotalDays) // Add null-forgiving operator
                    },
                    "approved" => new
                    {
                        Title = $"Approved Claims Report: {fromDate:dd MMM yyyy} to {toDate:dd MMM yyyy}",
                        ApprovedCount = filteredClaims.Count(c => c.Status == "approved" || c.Status == "auto-approved"),
                        TotalAmount = filteredClaims.Where(c => c.Status == "approved" || c.Status == "auto-approved").Sum(c => c.TotalAmount),
                        LecturersCount = filteredClaims.Where(c => c.Status == "approved" || c.Status == "auto-approved").Select(c => c.LecturerName).Distinct().Count(),
                        AverageAmount = filteredClaims.Where(c => c.Status == "approved" || c.Status == "auto-approved").Average(c => c.TotalAmount)
                    },
                    "lecturer" => new
                    {
                        Title = $"Lecturer Summary Report: {fromDate:dd MMM yyyy} to {toDate:dd MMM yyyy}",
                        LecturerCount = filteredClaims.Select(c => c.LecturerName).Distinct().Count(),
                        TotalClaims = filteredClaims.Count(),
                        ClaimsPerLecturer = filteredClaims.GroupBy(c => c.LecturerName)
                            .ToDictionary(g => g.Key, g => g.Count()),
                        TotalAmountPerLecturer = filteredClaims.Where(c => c.Status == "approved" || c.Status == "auto-approved")
                            .GroupBy(c => c.LecturerName)
                            .ToDictionary(g => g.Key, g => g.Sum(c => c.TotalAmount))
                    },
                    _ => new { Title = "General Report", Message = "Report generated successfully" }
                };

                return Json(new { success = true, data = report });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating report");
                return Json(new { success = false, message = "Error generating report" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ProcessPayments()
        {
            try
            {
                var claims = await _claimsRepository.GetAllAsync();
                var approvedClaims = claims.Where(c => c.Status == "approved" || c.Status == "auto-approved").ToList();

                if (approvedClaims.Count == 0)
                {
                    return Json(new { success = false, message = "No approved claims available for payment processing" });
                }

                var totalAmount = approvedClaims.Sum(c => c.TotalAmount);
                var lecturerCount = approvedClaims.Select(c => c.LecturerName).Distinct().Count();
                var claimCount = approvedClaims.Count;

                // Simulate payment processing
                await Task.Delay(2000); // Simulate processing time

                _logger.LogInformation("Processed payments for {ClaimCount} claims totaling R{TotalAmount} to {LecturerCount} lecturers",
                    claimCount, totalAmount, lecturerCount);

                return Json(new
                {
                    success = true,
                    message = $"Successfully processed payments for {claimCount} claims totaling R{totalAmount:F2} to {lecturerCount} lecturers.",
                    data = new { claimCount, totalAmount, lecturerCount }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payments");
                return Json(new { success = false, message = "Error processing payments" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetClaimsData()
        {
            try
            {
                var claims = await _claimsRepository.GetAllAsync();
                var monthlyData = claims
                    .GroupBy(c => new { c.SubmittedAt.Year, c.SubmittedAt.Month })
                    .Select(g => new
                    {
                        Period = $"{g.Key.Year}-{g.Key.Month:D2}",
                        TotalClaims = g.Count(),
                        ApprovedClaims = g.Count(c => c.Status == "approved" || c.Status == "auto-approved"),
                        TotalAmount = g.Where(c => c.Status == "approved" || c.Status == "auto-approved").Sum(c => c.TotalAmount)
                    })
                    .OrderBy(x => x.Period)
                    .ToList();

                return Json(new { success = true, data = monthlyData });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching claims data");
                return Json(new { success = false, message = "Error loading claims data" });
            }
        }
    }
}