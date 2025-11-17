namespace AcademicClaimHub.Models
{
    // Represents a lecturer in the system
    public class Lecturer
    {
        // Unique identifier for each lecturer
        public int LecturerID { get; set; }

        // Full name of the lecturer
        public string FullName { get; set; } = string.Empty;

        // Lecturer's hourly payment rate
        public double HourlyRate { get; set; }
    }
}
