using ClaimManagementHub.Models;
using ClaimManagementHub.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace ClaimManagementHub.Pages.Lecturer
{
    public class SubmitClaimModel : PageModel
    {
        private readonly IClaimsRepository _repo;
        private readonly IWebHostEnvironment _env;

        public SubmitClaimModel(IClaimsRepository repo, IWebHostEnvironment env)
        {
            _repo = repo;
            _env = env;
        }

        [BindProperty]
        public Claim Claim { get; set; } = new();

        [BindProperty]
        public IFormFile? Upload { get; set; }

        public string? SuccessMessage { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(Claim.LecturerName))
            {
                ModelState.AddModelError("Claim.LecturerName", "Lecturer name is required");
            }
            if (Claim.HoursWorked <= 0)
            {
                ModelState.AddModelError("Claim.HoursWorked", "Hours worked must be greater than 0");
            }
            if (Claim.HourlyRate <= 0)
            {
                ModelState.AddModelError("Claim.HourlyRate", "Hourly rate must be greater than 0");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Handle file upload
            if (Upload != null)
            {
                var allowed = new[] { ".pdf", ".docx", ".xlsx" };
                var ext = Path.GetExtension(Upload.FileName).ToLowerInvariant();
                if (!allowed.Contains(ext))
                {
                    ModelState.AddModelError("Upload", "Invalid file type. Allowed: .pdf, .docx, .xlsx");
                    return Page();
                }
                if (Upload.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("Upload", "File size must be less than 5MB");
                    return Page();
                }

                var uploadsDir = Path.Combine(_env.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsDir)) Directory.CreateDirectory(uploadsDir);

                var fileName = $"{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(uploadsDir, fileName);
                using (var stream = System.IO.File.Create(filePath))
                {
                    await Upload.CopyToAsync(stream);
                }

                Claim.FileName = Upload.FileName;
                Claim.FilePath = $"/uploads/{fileName}";
            }

            var created = await _repo.CreateAsync(Claim);
            SuccessMessage = $"Claim submitted successfully! Total amount: R{created.TotalAmount:F2}";
            ModelState.Clear();
            Claim = new();
            Upload = null;
            return Page();
        }
    }
}
