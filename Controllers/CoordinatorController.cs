using ClaimManagementHub.Models;
using ClaimManagementHub.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using ClaimManagementHub.Hubs;

namespace ClaimManagementHub.Controllers
{
    [Authorize(Policy = "CoordinatorOnly")]
    public class CoordinatorController : Controller
    {
        private readonly IClaimsRepository _repo;
        private readonly ILogger<CoordinatorController> _logger;
        private readonly IHubContext<ClaimHub> _hubContext;

        public CoordinatorController(IClaimsRepository repo, ILogger<CoordinatorController> logger, IHubContext<ClaimHub> hubContext)
        {
            _repo = repo;
            _logger = logger;
            _hubContext = hubContext;
        }

        // ... keep all existing methods until the Approve method ...

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

                claim.ProgressStatus = "Approved";
                claim.ReviewedBy = User.Identity?.Name;

                if (!string.IsNullOrEmpty(notes))
                {
                    claim.AdditionalNotes += $" [Coordinator Note: {notes}]";
                }

                // Notify lecturer via SignalR
                if (_hubContext != null)
                {
                    await _hubContext.Clients.Group(claim.TrackingId)
                        .SendAsync("StatusUpdated", new
                        {
                            ClaimId = claim.Id,
                            Status = claim.Status,
                            ProgressStatus = claim.ProgressStatus,
                            Message = "Your claim has been approved",
                            ReviewedBy = claim.ReviewedBy,
                            Timestamp = DateTime.UtcNow
                        });
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

        // ... rest of existing methods ...
    }
}