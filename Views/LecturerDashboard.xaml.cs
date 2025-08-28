using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AcademicClaimHub.Models;
using AcademicClaimHub.Data;

namespace AcademicClaimHub.Views
{
    /// <summary>
    /// Interaction logic for LecturerDashboard.xaml
    /// Handles lecturer claim submissions and input validation.
    /// </summary>
    public partial class LecturerDashboard : UserControl
    {
        // Counter used to assign unique claim IDs
        private static int _claimCounter = 1;

        // Regex pattern to validate numeric input (allows digits and one decimal point)
        private static readonly Regex _numRegex = new(@"^[0-9]*[.]?[0-9]*$");

        public LecturerDashboard()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles claim submission when the button is clicked.
        /// Performs validation and adds a claim to the repository.
        /// </summary>
        private void SubmitClaim_Click(object sender, RoutedEventArgs e)
        {
            // Validate lecturer name
            var name = txtLecturerName.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(name))
            {
                ShowError("Please enter the lecturer name.");
                txtLecturerName.Focus();
                return;
            }

            // Validate hours worked
            if (!double.TryParse(txtHours.Text, out var hours) || hours <= 0)
            {
                ShowError("Hours Worked must be a number greater than 0.");
                txtHours.Focus();
                return;
            }

            // Validate hourly rate
            if (!double.TryParse(txtRate.Text, out var rate) || rate <= 0)
            {
                ShowError("Hourly Rate must be a number greater than 0.");
                txtRate.Focus();
                return;
            }

            // Create a new claim object
            var claim = new Claim
            {
                ClaimID = _claimCounter++,
                LecturerName = name,
                HoursWorked = hours,
                HourlyRate = rate,
                // Status defaults to "Pending"
            };

            // Save the claim in memory
            ClaimRepository.AddClaim(claim);

            // Display success message in green
            lblStatus.Foreground = (System.Windows.Media.Brush)Application.Current.FindResource("SuccessColor");
            lblStatus.Text = $"Submitted ✔  Claim #{claim.ClaimID} | Amount: {claim.TotalAmount:C}";

            // Clear input fields for the next claim
            txtLecturerName.Text = "";
            txtHours.Text = "";
            txtRate.Text = "";
            txtLecturerName.Focus();
        }

        /// <summary>
        /// Displays an error message in red.
        /// </summary>
        private void ShowError(string message)
        {
            lblStatus.Foreground = (System.Windows.Media.Brush)Application.Current.FindResource("DangerColor");
            lblStatus.Text = "Error: " + message;
        }

        /// <summary>
        /// Ensures only numeric values (with optional single dot) can be typed.
        /// </summary>
        private void NumericOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var box = (TextBox)sender;
            var proposed = box.Text.Remove(box.SelectionStart, box.SelectionLength)
                                   .Insert(box.SelectionStart, e.Text);
            e.Handled = !_numRegex.IsMatch(proposed);
        }

        /// <summary>
        /// Ensures only numeric values can be pasted into the textbox.
        /// </summary>
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
