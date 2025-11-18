using ClaimManagementHub.Models;
using ClaimManagementHub.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace ClaimManagementHub.Pages.HR
{
    [Authorize(Policy = "HROnly")]
    public class DashboardModel : PageModel
    {
        private readonly IClaimsRepository _claimsRepository;
        private readonly InMemoryUserService _userService;

        public DashboardModel(IClaimsRepository claimsRepository, InMemoryUserService userService)
        {
            _claimsRepository = claimsRepository;
            _userService = userService;
        }

        public List<LecturerSummary> Lecturers { get; set; } = new();
        public PaymentSummary PaymentSummary { get; set; } = new();
        public string? GeneratedReport { get; set; }
        public string? ProcessedMessage { get; set; }

        public async Task OnGetAsync()
        {
            await LoadData();
        }

        public async Task<IActionResult> OnPostGenerateReportAsync(string reportType, DateTime fromDate, DateTime toDate)
        {
            var claims = await _claimsRepository.GetAllAsync();
            var filteredClaims = claims.Where(c => c.SubmittedAt >= fromDate && c.SubmittedAt <= toDate);

            GeneratedReport = reportType switch
            {
                "monthly" => $"Monthly Claims Report: {filteredClaims.Count()} claims between {fromDate:dd MMM yyyy} and {toDate:dd MMM yyyy}",
                "approved" => $"Approved Claims Report: {filteredClaims.Count(c => c.Status == "approved")} approved claims totaling R{filteredClaims.Where(c => c.Status == "approved").Sum(c => c.TotalAmount):F2}",
                "lecturer" => $"Lecturer Summary Report: Claims from {filteredClaims.Select(c => c.LecturerName).Distinct().Count()} different lecturers",
                _ => "Report generated successfully"
            };

            await LoadData();
            return Page();
        }

        public async Task<IActionResult> OnPostProcessPaymentsAsync()
        {
            var claims = await _claimsRepository.GetAllAsync();
            var approvedClaims = claims.Where(c => c.Status == "approved").ToList();

            // Simulate payment processing
            var totalAmount = approvedClaims.Sum(c => c.TotalAmount);
            var lecturerCount = approvedClaims.Select(c => c.LecturerName).Distinct().Count();

            ProcessedMessage = $"Successfully processed payments for {approvedClaims.Count} claims totaling R{totalAmount:F2} to {lecturerCount} lecturers.";

            await LoadData();
            return Page();
        }

        private async Task LoadData()
        {
            var claims = await _claimsRepository.GetAllAsync();
            var users = _userService.GetAllUsers();
            var lecturers = users.Where(u => u.Role == "Lecturer").ToList();

            // Build lecturer summaries
            Lecturers = lecturers.Select(lecturer => new LecturerSummary
            {
                FullName = lecturer.FullName,
                Email = lecturer.Email,
                TotalClaims = claims.Count(c => c.LecturerName == lecturer.FullName),
                TotalApproved = claims.Where(c => c.LecturerName == lecturer.FullName && c.Status == "approved").Sum(c => c.TotalAmount)
            }).ToList();

            // Build payment summary
            var approvedClaims = claims.Where(c => c.Status == "approved").ToList();
            PaymentSummary = new PaymentSummary
            {
                ReadyForPayment = approvedClaims.Count,
                TotalAmount = approvedClaims.Sum(c => c.TotalAmount),
                TotalLecturers = approvedClaims.Select(c => c.LecturerName).Distinct().Count(),
                ProcessedThisMonth = approvedClaims.Count(c => c.ReviewedAt.HasValue && c.ReviewedAt.Value.Month == DateTime.UtcNow.Month)
            };
        }
    }

    public class LecturerSummary
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int TotalClaims { get; set; }
        public double TotalApproved { get; set; }
    }

    public class PaymentSummary
    {
        public int ReadyForPayment { get; set; }
        public double TotalAmount { get; set; }
        public int TotalLecturers { get; set; }
        public int ProcessedThisMonth { get; set; }
    }
}