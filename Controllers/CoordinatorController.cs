using ClaimManagementHub.Models;
using ClaimManagementHub.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ClaimManagementHub.Controllers
{
    [Authorize(Policy = "CoordinatorOnly")]
    public class CoordinatorController : Controller
    {
        private readonly IClaimsRepository _repo;
        private readonly ILogger<CoordinatorController> _logger;

        public CoordinatorController(IClaimsRepository repo, ILogger<CoordinatorController> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> VerifyClaims(string filter = "all", string sortBy = "newest")
        {
            try
            {
                var allClaims = (await _repo.GetAllAsync()).ToList();
                var workflowAnalysis = await _repo.GetWorkflowAnalysisAsync();

                // Filter claims
                var filteredClaims = filter switch
                {
                    "pending" => allClaims.Where(c => c.Status == "pending"),
                    "approved" => allClaims.Where(c => c.Status == "approved" || c.Status == "auto-approved"),
                    "rejected" => allClaims.Where(c => c.Status == "rejected"),
                    "auto-approved" => allClaims.Where(c => c.Status == "auto-approved"),
                    "flagged" => allClaims.Where(c => c.AdditionalNotes?.Contains("[FLAGGED:") == true),
                    _ => allClaims
                };

                // Sort claims
                var sortedClaims = sortBy switch
                {
                    "oldest" => filteredClaims.OrderBy(c => c.SubmittedAt),
                    "amount-high" => filteredClaims.OrderByDescending(c => c.TotalAmount),
                    "amount-low" => filteredClaims.OrderBy(c => c.TotalAmount),
                    "name" => filteredClaims.OrderBy(c => c.LecturerName),
                    _ => filteredClaims.OrderByDescending(c => c.SubmittedAt)
                };

                var statusCounts = new
                {
                    All = allClaims.Count,
                    Pending = allClaims.Count(c => c.Status == "pending"),
                    Approved = allClaims.Count(c => c.Status == "approved" || c.Status == "auto-approved"),
                    Rejected = allClaims.Count(c => c.Status == "rejected"),
                    AutoApproved = allClaims.Count(c => c.Status == "auto-approved"),
                    Flagged = allClaims.Count(c => c.AdditionalNotes?.Contains("[FLAGGED:") == true)
                };

                ViewBag.Filter = filter;
                ViewBag.SortBy = sortBy;
                ViewBag.StatusCounts = statusCounts;
                ViewBag.WorkflowAnalysis = workflowAnalysis;
                ViewBag.FilterOptions = new SelectList(new[]
                {
                    new { Value = "all", Text = "All Claims" },
                    new { Value = "pending", Text = "Pending Review" },
                    new { Value = "approved", Text = "Approved" },
                    new { Value = "auto-approved", Text = "Auto-Approved" },
                    new { Value = "rejected", Text = "Rejected" },
                    new { Value = "flagged", Text = "Flagged Claims" }
                }, "Value", "Text", filter);

                ViewBag.SortOptions = new SelectList(new[]
                {
                    new { Value = "newest", Text = "Newest First" },
                    new { Value = "oldest", Text = "Oldest First" },
                    new { Value = "amount-high", Text = "Amount (High to Low)" },
                    new { Value = "amount-low", Text = "Amount (Low to High)" },
                    new { Value = "name", Text = "Lecturer Name" }
                }, "Value", "Text", sortBy);

                return View(sortedClaims);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading claims for verification");
                TempData["ErrorMessage"] = "Unable to load claims. Please try again.";
                return View(Enumerable.Empty<Claim>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> Approve(int id, string notes = "")
        {
            try
            {
                var claim = await _repo.UpdateStatusAsync(id, "approved");
                if (claim == null)
                {
                    return Json(new { success = false, message = "Claim not found" });
                }

                if (!string.IsNullOrEmpty(notes))
                {
                    claim.AdditionalNotes += $" [Coordinator Note: {notes}]";
                }

                _logger.LogInformation("Claim {ClaimId} approved by coordinator {Coordinator}", id, User.Identity?.Name);
                return Json(new { success = true, message = "Claim approved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving claim {ClaimId}", id);
                return Json(new { success = false, message = "Error approving claim" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Reject(int id, string reason)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(reason))
                {
                    return Json(new { success = false, message = "Rejection reason is required" });
                }

                var claim = await _repo.UpdateStatusAsync(id, "rejected");
                if (claim == null)
                {
                    return Json(new { success = false, message = "Claim not found" });
                }

                claim.AdditionalNotes += $" [Rejection Reason: {reason}]";

                _logger.LogInformation("Claim {ClaimId} rejected by coordinator {Coordinator}", id, User.Identity?.Name);
                return Json(new { success = true, message = "Claim rejected successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting claim {ClaimId}", id);
                return Json(new { success = false, message = "Error rejecting claim" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetClaimDetails(int id)
        {
            try
            {
                var claim = await _repo.GetByIdAsync(id);
                if (claim == null)
                {
                    return Json(new { success = false, message = "Claim not found" });
                }

                var details = new
                {
                    claim.Id,
                    claim.LecturerName,
                    claim.HoursWorked,
                    claim.HourlyRate,
                    claim.TotalAmount,
                    claim.AdditionalNotes,
                    claim.Status,
                    claim.FileName,
                    claim.FilePath,
                    SubmittedAt = claim.SubmittedAt.ToString("yyyy-MM-dd HH:mm"),
                    ReviewedAt = claim.ReviewedAt?.ToString("yyyy-MM-dd HH:mm"),
                    IsFlagged = claim.AdditionalNotes?.Contains("[FLAGGED:") == true,
                    IsAutoApproved = claim.Status == "auto-approved"
                };

                return Json(new { success = true, data = details });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching claim details for {ClaimId}", id);
                return Json(new { success = false, message = "Error loading claim details" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> BulkApprove(int[] claimIds)
        {
            try
            {
                if (claimIds == null || claimIds.Length == 0)
                {
                    return Json(new { success = false, message = "No claims selected" });
                }

                var approvedCount = 0;
                foreach (var id in claimIds)
                {
                    var claim = await _repo.UpdateStatusAsync(id, "approved");
                    if (claim != null)
                    {
                        approvedCount++;
                    }
                }

                _logger.LogInformation("Bulk approved {Count} claims by coordinator {Coordinator}", approvedCount, User.Identity?.Name);
                return Json(new { success = true, message = $"Successfully approved {approvedCount} claims" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in bulk approval for {Count} claims", claimIds?.Length ?? 0);
                return Json(new { success = false, message = "Error processing bulk approval" });
            }
        }
    }
}