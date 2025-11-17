<<<<<<< HEAD
﻿using System;

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
        public string Status { get; set; } = "pending"; // pending, approved, rejected
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReviewedAt { get; set; }
=======
﻿namespace AcademicClaimHub.Models
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
>>>>>>> b722ba800dd2ad9fca1210522750d06c171c9c9d
    }
}
