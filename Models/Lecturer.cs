namespace AcademicClaimHub.Models
{
    public class Lecturer
    {
        public int LecturerID { get; set; }
        public string FullName { get; set; } = string.Empty; // Default empty
        public double HourlyRate { get; set; }
    }
}
