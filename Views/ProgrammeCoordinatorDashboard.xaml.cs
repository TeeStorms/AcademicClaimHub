using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AcademicClaimHub.Data;
using AcademicClaimHub.Models;

namespace AcademicClaimHub.Views
{
    // Dashboard view for Programme Coordinators
    public partial class ProgrammeCoordinatorDashboard : UserControl
    {
        public ProgrammeCoordinatorDashboard()
        {
            InitializeComponent();

            // Set a default filter when the dashboard loads
            if (cbStatusFilter != null)
                cbStatusFilter.SelectedIndex = 0;

            // Load claims once the page is fully loaded
            Loaded += (s, e) => LoadClaims();
        }

        // Get the current filter option from the dropdown
        private string GetSelectedFilter()
        {
            if (cbStatusFilter == null)
                return "All"; // Default filter if dropdown is not set

            var item = cbStatusFilter.SelectedItem as ComboBoxItem;
            return item?.Content?.ToString() ?? "All";
        }

        // Load and display claims in the table based on the filter
        private void LoadClaims()
        {
            // Use empty list if Claims data is missing
            IEnumerable<Claim> data = ClaimRepository.Claims ?? Enumerable.Empty<Claim>();
            var filter = GetSelectedFilter();

            // Apply filter if not set to "All"
            if (filter != "All")
                data = data.Where(c => c.Status == filter);

            // Refresh the data grid with claims
            if (dgClaims != null)
            {
                dgClaims.ItemsSource = null;
                dgClaims.ItemsSource = data.ToList();
            }
        }

        // Approve the selected claim
        private void ApproveClaim_Click(object sender, RoutedEventArgs e)
        {
            if (dgClaims.SelectedItem is Claim claim)
            {
                claim.Status = "Approved";
                lblStatus.Text = $"✅ Claim {claim.ClaimID} approved.";
                LoadClaims(); // Refresh list
            }
            else
            {
                lblStatus.Text = "⚠ Please select a claim to approve.";
            }
        }

        // Reject the selected claim
        private void RejectClaim_Click(object sender, RoutedEventArgs e)
        {
            if (dgClaims.SelectedItem is Claim claim)
            {
                claim.Status = "Rejected";
                lblStatus.Text = $"❌ Claim {claim.ClaimID} rejected.";
                LoadClaims(); // Refresh list
            }
            else
            {
                lblStatus.Text = "⚠ Please select a claim to reject.";
            }
        }

        // Refresh claim list when filter changes
        private void CbStatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadClaims();
        }

        // Refresh claim list when refresh button is clicked
        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadClaims();
        }
    }
}
