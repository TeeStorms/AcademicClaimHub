using ClaimManagementHub.Models;
using ClaimManagementHub.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClaimManagementHub.Controllers
{
    [Authorize(Policy = "LecturerOnly")]
    public class LecturerController : Controller
    {
        private readonly IClaimsRepository _repo;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<LecturerController> _logger;
        private readonly ApprovalWorkflowService _workflowService;

        // Static arrays for file validation
        private static readonly string[] AllowedFileExtensions = { ".pdf", ".docx", ".xlsx", ".jpg", ".jpeg", ".png" };

        public LecturerController(IClaimsRepository repo, IWebHostEnvironment env, ILogger<LecturerController> logger)
        {
            _repo = repo;
            _env = env;
            _logger = logger;
            _workflowService = new ApprovalWorkflowService();
        }

        [HttpGet]
        public IActionResult SubmitClaim()
        {
            var defaultClaim = new Claim
            {
                LecturerName = User.Identity?.Name ?? string.Empty
            };
            return View(defaultClaim);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitClaim(Claim claim, IFormFile upload)
        {
            try
            {
                // Set lecturer name from authenticated user
                claim.LecturerName = User.Identity?.Name ?? string.Empty;

                // Basic validation
                if (string.IsNullOrWhiteSpace(claim.LecturerName))
                {
                    ModelState.AddModelError("LecturerName", "Lecturer name is required");
                }
                if (claim.HoursWorked <= 0)
                {
                    ModelState.AddModelError("HoursWorked", "Hours worked must be greater than 0");
                }
                if (claim.HourlyRate <= 0)
                {
                    ModelState.AddModelError("HourlyRate", "Hourly rate must be greater than 0");
                }

                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
                }

                // Handle file upload
                if (upload != null && upload.Length > 0)
                {
                    var ext = Path.GetExtension(upload.FileName).ToLowerInvariant();
                    if (!AllowedFileExtensions.Contains(ext))
                    {
                        return Json(new { success = false, errors = new[] { "Invalid file type. Allowed: .pdf, .docx, .xlsx, .jpg, .jpeg, .png" } });
                    }
                    if (upload.Length > 5 * 1024 * 1024)
                    {
                        return Json(new { success = false, errors = new[] { "File size must be less than 5MB" } });
                    }

                    var uploadsDir = Path.Combine(_env.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadsDir))
                        Directory.CreateDirectory(uploadsDir);

                    var fileName = $"{Guid.NewGuid()}{ext}";
                    var filePath = Path.Combine(uploadsDir, fileName);
                    using (var stream = System.IO.File.Create(filePath))
                    {
                        await upload.CopyToAsync(stream);
                    }

                    claim.FileName = upload.FileName;
                    claim.FilePath = $"/uploads/{fileName}";
                }

                // Process through workflow
                var workflowResult = _workflowService.ProcessClaim(claim);

                var created = await _repo.CreateAsync(claim);

                _logger.LogInformation("Claim submitted by {Lecturer} for R{Amount}", claim.LecturerName, claim.TotalAmount);

                return Json(new
                {
                    success = true,
                    message = $"Claim submitted successfully! Total amount: R{created.TotalAmount:F2}",
                    isAutoApproved = workflowResult.IsAutoApproved,
                    flags = workflowResult.Flags,
                    claimId = created.Id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting claim for lecturer {Lecturer}", User.Identity?.Name);
                return Json(new { success = false, errors = new[] { "An error occurred while submitting the claim. Please try again." } });
            }
        }

        [HttpGet]
        public async Task<IActionResult> MyClaims()
        {
            try
            {
                var allClaims = await _repo.GetAllAsync();
                var myClaims = allClaims.Where(c => c.LecturerName == User.Identity?.Name)
                                      .OrderByDescending(c => c.SubmittedAt)
                                      .ToList();

                return View(myClaims);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading claims for lecturer {Lecturer}", User.Identity?.Name);
                TempData["ErrorMessage"] = "Unable to load your claims. Please try again.";
                return View(new List<Claim>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetClaimStatus(int id)
        {
            try
            {
                var claim = await _repo.GetByIdAsync(id);
                if (claim == null || claim.LecturerName != User.Identity?.Name)
                {
                    return Json(new { success = false, message = "Claim not found" });
                }

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        claim.Status,
                        claim.SubmittedAt,
                        claim.ReviewedAt,
                        claim.TotalAmount,
                        claim.AdditionalNotes
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching claim status for claim {ClaimId}", id);
                return Json(new { success = false, message = "Error loading claim status" });
            }
        }
    }
}