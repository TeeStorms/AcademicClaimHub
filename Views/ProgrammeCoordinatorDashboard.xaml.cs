using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AcademicClaimHub.Data;
using AcademicClaimHub.Models;

namespace AcademicClaimHub.Views
{
    public partial class ProgrammeCoordinatorDashboard : UserControl
    {
        public ProgrammeCoordinatorDashboard()
        {
            InitializeComponent();

            // Set default filter if null
            if (cbStatusFilter != null)
                cbStatusFilter.SelectedIndex = 0;

            // Load claims after UI fully loaded
            Loaded += (s, e) => LoadClaims();
        }

        private string GetSelectedFilter()
        {
            if (cbStatusFilter == null)
                return "All"; // Safe fallback

            var item = cbStatusFilter.SelectedItem as ComboBoxItem;
            return item?.Content?.ToString() ?? "All";
        }

        private void LoadClaims()
        {
            // Safely handle null Claims collection
            IEnumerable<Claim> data = ClaimRepository.Claims ?? Enumerable.Empty<Claim>();
            var filter = GetSelectedFilter();

            if (filter != "All")
                data = data.Where(c => c.Status == filter);

            if (dgClaims != null)
            {
                dgClaims.ItemsSource = null;
                dgClaims.ItemsSource = data.ToList();
            }
        }

        private void ApproveClaim_Click(object sender, RoutedEventArgs e)
        {
            if (dgClaims.SelectedItem is Claim claim)
            {
                claim.Status = "Approved";
                lblStatus.Text = $"✅ Claim {claim.ClaimID} approved.";
                LoadClaims();
            }
            else
            {
                lblStatus.Text = "⚠ Please select a claim to approve.";
            }
        }

        private void RejectClaim_Click(object sender, RoutedEventArgs e)
        {
            if (dgClaims.SelectedItem is Claim claim)
            {
                claim.Status = "Rejected";
                lblStatus.Text = $"❌ Claim {claim.ClaimID} rejected.";
                LoadClaims();
            }
            else
            {
                lblStatus.Text = "⚠ Please select a claim to reject.";
            }
        }

        private void CbStatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadClaims();
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadClaims();
        }
    }
}
