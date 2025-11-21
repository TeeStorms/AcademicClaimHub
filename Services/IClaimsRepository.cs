using ClaimManagementHub.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClaimManagementHub.Services
{
    public interface IClaimsRepository
    {
        Task<IEnumerable<Claim>> GetAllAsync();
        Task<Claim?> GetByIdAsync(int id);
        Task<Claim> CreateAsync(Claim claim);
        Task<Claim?> UpdateStatusAsync(int id, string status);
        Task<IEnumerable<Claim>> GetRecentAsync(int count);
        Task<ClaimSummary> GetSummaryAsync();
        Task<WorkflowAnalysis> GetWorkflowAnalysisAsync();
    }

    public class ClaimSummary
    {
        public int TotalClaims { get; set; }
        public int PendingClaims { get; set; }
        public int ApprovedClaims { get; set; }
        public int RejectedClaims { get; set; }
        public int AutoApprovedClaims { get; set; }
        public double TotalAmountApproved { get; set; }
        public int ProcessedThisMonth { get; set; } 
    }
}