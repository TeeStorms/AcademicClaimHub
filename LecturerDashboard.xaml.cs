using System;
using System.Windows;
using System.Windows.Controls;
using AcademicClaimHub.Models;
using AcademicClaimHub.Data;

namespace AcademicClaimHub.Views
{
    public partial class LecturerDashboard : UserControl
    {
        private static int _claimCounter = 1;

        public LecturerDashboard()
        {
            InitializeComponent();
        }

        private void SubmitClaim_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var claim = new Claim
                {
                    ClaimID = _claimCounter++,
                    LecturerName = txtLecturerName.Text,
                    HoursWorked = double.Parse(txtHours.Text),
                    HourlyRate = double.Parse(txtRate.Text)
                };

                ClaimRepository.AddClaim(claim);

                lblStatus.Text = $"Claim submitted successfully! (ID: {claim.ClaimID}, Amount: {claim.TotalAmount:C})";
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"Error: {ex.Message}";
            }
        }
    }
}
