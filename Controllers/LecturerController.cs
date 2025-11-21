using ClaimManagementHub.Models;
using ClaimManagementHub.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using ClaimManagementHub.Hubs;

namespace ClaimManagementHub.Controllers
{
    [Authorize(Policy = "LecturerOnly")]
    public class LecturerController : Controller
    {
        private readonly IClaimsRepository _repo;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<LecturerController> _logger;
        private readonly ApprovalWorkflowService _workflowService;
        private readonly FileUploadService _fileUploadService;
        private readonly IHubContext<ClaimHub> _hubContext;

        public LecturerController(
            IClaimsRepository repo,
            IWebHostEnvironment env,
            ILogger<LecturerController> logger,
            FileUploadService fileUploadService,
            IHubContext<ClaimHub> hubContext)
        {
            _repo = repo;
            _env = env;
            _logger = logger;
            _workflowService = new ApprovalWorkflowService();
            _fileUploadService = fileUploadService;
            _hubContext = hubContext;
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitClaim(Claim claim, IFormFile? supportingDocument)
        {
            try
            {
                // Set lecturer name from authenticated user
                claim.LecturerName = User.Identity?.Name ?? string.Empty;
                claim.TrackingId = Guid.NewGuid().ToString();
                claim.ProgressStatus = "Submitted";

                // Enhanced validation
                var validationErrors = new List<string>();

                if (string.IsNullOrWhiteSpace(claim.LecturerName))
                    validationErrors.Add("Lecturer name is required");

                if (claim.HoursWorked <= 0)
                    validationErrors.Add("Hours worked must be greater than 0");
                else if (claim.HoursWorked > 100)
                    validationErrors.Add("Hours worked cannot exceed 100 hours");

                if (claim.HourlyRate <= 0)
                    validationErrors.Add("Hourly rate must be greater than 0");
                else if (claim.HourlyRate > 500)
                    validationErrors.Add("Hourly rate cannot exceed R500 per hour");

                if (validationErrors.Count > 0)
                {
                    return Json(new { success = false, errors = validationErrors });
                }

                // Auto-calculate total amount
                claim.TotalAmount = Math.Round(claim.HoursWorked * claim.HourlyRate, 2);

                // Handle file upload with enhanced service
                if (supportingDocument != null && supportingDocument.Length > 0)
                {
                    var uploadResult = await _fileUploadService.UploadFileAsync(supportingDocument, "supporting-docs");
                    if (!uploadResult.Success)
                    {
                        return Json(new { success = false, errors = new[] { uploadResult.Error } });
                    }

                    claim.FileName = uploadResult.FileName;
                    claim.FilePath = uploadResult.FilePath;
                }

                // Process through workflow
                var workflowResult = _workflowService.ProcessClaim(claim);

                // Update progress status based on workflow result
                if (workflowResult.IsAutoApproved)
                {
                    claim.ProgressStatus = "Auto-Approved";
                    claim.Status = "auto-approved";
                    claim.ReviewedAt = DateTime.UtcNow;
                    claim.ReviewedBy = "System";
                }
                else
                {
                    claim.ProgressStatus = "Under Review";
                }

                var created = await _repo.CreateAsync(claim);

                // Notify coordinators via SignalR
                if (_hubContext != null)
                {
                    await _hubContext.Clients.Group("Coordinators")
                        .SendAsync("NewClaimSubmitted", new
                        {
                            ClaimId = created.Id,
                            LecturerName = created.LecturerName,
                            Amount = created.TotalAmount,
                            SubmittedAt = created.SubmittedAt,
                            Status = created.Status,
                            IsAutoApproved = workflowResult.IsAutoApproved,
                            Flags = workflowResult.Flags
                        });
                }

                _logger.LogInformation("Claim {ClaimId} submitted by {Lecturer} for R{Amount}",
                    created.Id, claim.LecturerName, claim.TotalAmount);

                return Json(new
                {
                    success = true,
                    message = $"Claim submitted successfully! Total amount: R{created.TotalAmount:F2}",
                    isAutoApproved = workflowResult.IsAutoApproved,
                    flags = workflowResult.Flags,
                    claimId = created.Id,
                    trackingId = created.TrackingId,
                    progressStatus = created.ProgressStatus
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting claim for lecturer {Lecturer}", User.Identity?.Name);
                return Json(new
                {
                    success = false,
                    errors = new[] { "An error occurred while submitting the claim. Please try again." }
                });
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
                        claim.ProgressStatus,
                        claim.SubmittedAt,
                        claim.ReviewedAt,
                        claim.TotalAmount,
                        claim.AdditionalNotes,
                        claim.TrackingId,
                        claim.ReviewedBy,
                        claim.RejectionReason
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching claim status for claim {ClaimId}", id);
                return Json(new { success = false, message = "Error loading claim status" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> TrackClaim(string trackingId)
        {
            try
            {
                var allClaims = await _repo.GetAllAsync();
                var claim = allClaims.FirstOrDefault(c => c.TrackingId == trackingId &&
                                                         c.LecturerName == User.Identity?.Name);

                if (claim == null)
                {
                    TempData["ErrorMessage"] = "Claim not found or you don't have permission to view it.";
                    return RedirectToAction("MyClaims");
                }

                return View(claim);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking claim {TrackingId}", trackingId);
                TempData["ErrorMessage"] = "Error loading claim details.";
                return RedirectToAction("MyClaims");
            }
        }
    }
}