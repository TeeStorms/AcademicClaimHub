namespace AcademicClaimHub.Models
{
    // Represents a lecturer's claim for hours worked
    public class Claim
    {
        // Unique identifier for each claim
        public int ClaimID { get; set; }

        // Full name of the lecturer submitting the claim
        public string LecturerName { get; set; } = string.Empty;

        // Number of hours worked by the lecturer
        public double HoursWorked { get; set; }

        // Hourly rate agreed upon for the lecturer
        public double HourlyRate { get; set; }

        // Automatically calculated total amount (HoursWorked * HourlyRate)
        public double TotalAmount => HoursWorked * HourlyRate;

        // Current status of the claim (default is "Pending")
        public string Status { get; set; } = "Pending";
    }
}
