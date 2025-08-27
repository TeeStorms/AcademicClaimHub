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
            LoadClaims();
        }

        private void LoadClaims()
        {
            dgClaims.ItemsSource = null;
            dgClaims.ItemsSource = ClaimRepository.Claims;
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
    }
}
