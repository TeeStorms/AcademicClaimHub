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
            InitializeComponent();
        }

        private void ViewStats_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            int totalClaims = ClaimRepository.Claims.Count;
            double totalAmount = ClaimRepository.Claims.Sum(c => c.TotalAmount);
            int approved = ClaimRepository.Claims.Count(c => c.Status == "Approved");
            int rejected = ClaimRepository.Claims.Count(c => c.Status == "Rejected");

            var stats = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Total Claims", totalClaims.ToString()),
                new KeyValuePair<string, string>("Total Amount", totalAmount.ToString("C")),
                new KeyValuePair<string, string>("Approved", approved.ToString()),
                new KeyValuePair<string, string>("Rejected", rejected.ToString())
            };

            StatsGrid.ItemsSource = stats;
        }
    }
}
