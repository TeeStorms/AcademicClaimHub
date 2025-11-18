using ClaimManagementHub.Models;
using ClaimManagementHub.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClaimManagementHub.Pages.Dashboard
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IClaimsRepository _repo;
        public ClaimSummary Summary { get; set; } = new();
        public IEnumerable<Claim> Recent { get; set; } = new List<Claim>();
        public WorkflowAnalysis WorkflowAnalysis { get; set; } = new();

        public IndexModel(IClaimsRepository repo) => _repo = repo;

        public async Task OnGetAsync()
        {
            Summary = await _repo.GetSummaryAsync();
            Recent = await _repo.GetRecentAsync(5);
            WorkflowAnalysis = await _repo.GetWorkflowAnalysisAsync();
        }
    }
}