using System;

namespace ClaimManagementHub.Models
{
    public class Claim
    {
        public int Id { get; set; }
        public string LecturerName { get; set; } = string.Empty;
        public double HoursWorked { get; set; }
        public double HourlyRate { get; set; }
        public double TotalAmount { get; set; }
        public string? AdditionalNotes { get; set; }
        public string? FileName { get; set; }
        public string? FilePath { get; set; }
        public string Status { get; set; } = "pending"; // pending, approved, rejected, auto-approved
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReviewedAt { get; set; }
    }
}