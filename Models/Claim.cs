namespace AcademicClaimHub.Models
{
    public class Claim
    {
        public int ClaimID { get; set; }
        public string LecturerName { get; set; } = string.Empty; // Default empty
        public double HoursWorked { get; set; }
        public double HourlyRate { get; set; }
        public double TotalAmount => HoursWorked * HourlyRate;
        public string Status { get; set; } = "Pending"; // Default status
    }
}
