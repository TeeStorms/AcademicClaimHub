using ClaimManagementHub.Models;
using ClaimManagementHub.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClaimManagementHub.Pages.Dashboard
{
    public class IndexModel : PageModel
    {
        private readonly IClaimsRepository _repo;
        public ClaimSummary Summary { get; set; } = new();
        public IEnumerable<Claim> Recent { get; set; } = new List<Claim>();

        public IndexModel(IClaimsRepository repo) => _repo = repo;

        public async Task OnGetAsync()
        {
            Summary = await _repo.GetSummaryAsync();
            Recent = await _repo.GetRecentAsync(5);
        }
    }
}
