using ClaimManagementHub.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

public class IndexModel : PageModel
{
    private readonly IClaimsRepository _repo;
    public ClaimManagementHub.Services.ClaimSummary Summary { get; set; } = new();

    public IndexModel(IClaimsRepository repo)
    {
        _repo = repo;
    }

    public async Task OnGetAsync()
    {
        Summary = await _repo.GetSummaryAsync();
    }
}
