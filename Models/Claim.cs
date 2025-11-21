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

        // New properties for enhanced tracking
        public string? RejectionReason { get; set; }
        public string? ReviewedBy { get; set; }
        public string TrackingId { get; set; } = Guid.NewGuid().ToString();
        public string ProgressStatus { get; set; } = "Submitted"; // Submitted → Under Review → Approved/Rejected
    }
}