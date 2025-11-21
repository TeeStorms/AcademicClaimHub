using Microsoft.AspNetCore.SignalR;
using ClaimManagementHub.Models;
using System.Threading.Tasks;

namespace ClaimManagementHub.Hubs
{
    public class ClaimHub : Hub
    {
        public async Task JoinClaimGroup(string claimId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, claimId);
        }

        public async Task LeaveClaimGroup(string claimId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, claimId);
        }

        public async Task NotifyStatusUpdate(string claimId, string status, string message)
        {
            await Clients.Group(claimId).SendAsync("StatusUpdated", new
            {
                ClaimId = claimId,
                Status = status,
                Message = message,
                Timestamp = DateTime.UtcNow
            });
        }

        public async Task NotifyNewClaim(Claim claim)
        {
            await Clients.Group("Coordinators").SendAsync("NewClaimSubmitted", new
            {
                ClaimId = claim.Id,
                LecturerName = claim.LecturerName,
                Amount = claim.TotalAmount,
                SubmittedAt = claim.SubmittedAt,
                Status = claim.Status
            });
        }

        public async Task NotifyClaimApproval(int claimId, string approvedBy)
        {
            await Clients.All.SendAsync("ClaimApproved", new
            {
                ClaimId = claimId,
                ApprovedBy = approvedBy,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}