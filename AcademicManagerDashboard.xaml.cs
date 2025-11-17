using System.Linq;
using System.Windows.Controls;
using AcademicClaimHub.Data;
using System.Collections.Generic;

namespace AcademicClaimHub.Views
{
    public partial class AcademicManagerDashboard : UserControl
    {
        public AcademicManagerDashboard()
        {
            InitializeComponent(); // Initialize UI components
        }

        private void ViewStats_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var claims = ClaimRepository.Claims; // Get all in-memory claims

            // Calculate summary statistics
            int totalClaims = claims.Count;
            double totalAmount = claims.Sum(c => c.TotalAmount);
            int approved = claims.Count(c => c.Status == "Approved");
            int rejected = claims.Count(c => c.Status == "Rejected");
            int pending = claims.Count(c => c.Status == "Pending");

            // Populate overall summary grid
            var stats = new List<KeyValuePair<string, string>>
            {
                new("Total Claims", totalClaims.ToString()),
                new("Total Amount", totalAmount.ToString("C")),
                new("Approved", approved.ToString()),
                new("Rejected", rejected.ToString()),
                new("Pending", pending.ToString())
            };
            StatsGrid.ItemsSource = stats;

            // Populate status breakdown grid
            var statusBreakdown = claims
                .GroupBy(c => c.Status)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count(),
                    TotalAmount = g.Sum(x => x.TotalAmount).ToString("C")
                })
                .OrderByDescending(x => x.Count)
                .ToList();
            StatusBreakdownGrid.ItemsSource = statusBreakdown;

            // Populate per-lecturer summary grid
            var perLecturer = claims
                .GroupBy(c => c.LecturerName)
                .Select(g => new
                {
                    LecturerName = g.Key,
                    Count = g.Count(),
                    TotalHours = g.Sum(x => x.HoursWorked),
                    TotalAmount = g.Sum(x => x.TotalAmount).ToString("C")
                })
                .OrderByDescending(x => x.TotalAmount) // Sorts by total amount (string)
                .ToList();
            PerLecturerGrid.ItemsSource = perLecturer;
        }
    }
}
