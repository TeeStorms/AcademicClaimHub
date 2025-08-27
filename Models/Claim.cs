namespace AcademicClaimHub.Models
{
    public class Claim
    {
        public int ClaimID { get; set; }
        public string LecturerName { get; set; } = string.Empty;
        public double HoursWorked { get; set; }
        public double HourlyRate { get; set; }
        public double TotalAmount => HoursWorked * HourlyRate;
        public string Status { get; set; } = "Pending";
    }
}
