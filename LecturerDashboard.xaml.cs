using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AcademicClaimHub.Models;
using AcademicClaimHub.Data;

namespace AcademicClaimHub.Views
{
    public partial class LecturerDashboard : UserControl
    {
        private static int _claimCounter = 1;
        private static readonly Regex _numRegex = new(@"^[0-9]*[.]?[0-9]*$");

        public LecturerDashboard()
        {
            InitializeComponent();
        }

        private void SubmitClaim_Click(object sender, RoutedEventArgs e)
        {
            // Basic validations
            var name = txtLecturerName.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(name))
            {
                ShowError("Please enter the lecturer name.");
                txtLecturerName.Focus();
                return;
            }

            if (!double.TryParse(txtHours.Text, out var hours) || hours <= 0)
            {
                ShowError("Hours Worked must be a number greater than 0.");
                txtHours.Focus();
                return;
            }

            if (!double.TryParse(txtRate.Text, out var rate) || rate <= 0)
            {
                ShowError("Hourly Rate must be a number greater than 0.");
                txtRate.Focus();
                return;
            }

            var claim = new Claim
            {
                ClaimID = _claimCounter++,
                LecturerName = name,
                HoursWorked = hours,
                HourlyRate = rate,
                // Status defaults to "Pending"
            };

            ClaimRepository.AddClaim(claim);

            lblStatus.Foreground = (System.Windows.Media.Brush)Application.Current.FindResource("SuccessColor");
            lblStatus.Text = $"Submitted ✔  Claim #{claim.ClaimID} | Amount: {claim.TotalAmount:C}";

            // Clear fields for next entry
            txtLecturerName.Text = "";
            txtHours.Text = "";
            txtRate.Text = "";
            txtLecturerName.Focus();
        }

        private void ShowError(string message)
        {
            lblStatus.Foreground = (System.Windows.Media.Brush)Application.Current.FindResource("DangerColor");
            lblStatus.Text = "Error: " + message;
        }

        // Allow only digits and a single dot
        private void NumericOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var box = (TextBox)sender;
            var proposed = box.Text.Remove(box.SelectionStart, box.SelectionLength)
                                   .Insert(box.SelectionStart, e.Text);
            e.Handled = !_numRegex.IsMatch(proposed);
        }

        private void NumericOnly_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                var text = (string)e.DataObject.GetData(typeof(string));
                if (!_numRegex.IsMatch(text))
                    e.CancelCommand();
            }
            else
            {
                e.CancelCommand();
            }
        }
    }
}
