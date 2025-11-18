using ClaimManagementHub.Models;
using ClaimManagementHub.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClaimManagementHub.Pages.Coordinator
{
    [Authorize(Policy = "CoordinatorOnly")]
    public class VerifyClaimsModel : PageModel
    {
        private readonly IClaimsRepository _repo;
        public VerifyClaimsModel(IClaimsRepository repo) => _repo = repo;

        [BindProperty(SupportsGet = true)]
        public string Filter { get; set; } = "all";

        public IEnumerable<Claim> Claims { get; set; } = Enumerable.Empty<Claim>();

        public (int All, int Pending, int Approved, int Rejected) StatusCounts { get; set; }

        public async Task OnGetAsync()
        {
            var all = (await _repo.GetAllAsync()).ToList();
            StatusCounts = (all.Count, all.Count(c => c.Status == "pending"), all.Count(c => c.Status == "approved"), all.Count(c => c.Status == "rejected"));

            Claims = Filter switch
            {
                "pending" => all.Where(c => c.Status == "pending"),
                "approved" => all.Where(c => c.Status == "approved"),
                "rejected" => all.Where(c => c.Status == "rejected"),
                _ => all
            };
        }

        public async Task<IActionResult> OnPostApproveAsync(int id)
        {
            await _repo.UpdateStatusAsync(id, "approved");
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRejectAsync(int id)
        {
            await _repo.UpdateStatusAsync(id, "rejected");
            return RedirectToPage();
        }
    }
}
