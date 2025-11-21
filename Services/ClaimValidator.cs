using ClaimManagementHub.Models;

namespace ClaimManagementHub.Services
{
    public class ClaimValidator
    {
        public ValidationResult Validate(Claim claim)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(claim.LecturerName))
                errors.Add("Lecturer name is required");
            else if (claim.LecturerName.Length < 2 || claim.LecturerName.Length > 100)
                errors.Add("Lecturer name must be between 2 and 100 characters");

            if (claim.HoursWorked <= 0)
                errors.Add("Hours worked must be greater than 0");
            else if (claim.HoursWorked > 100)
                errors.Add("Hours worked cannot exceed 100 hours per claim");

            if (claim.HourlyRate <= 0)
                errors.Add("Hourly rate must be greater than 0");
            else if (claim.HourlyRate > 500)
                errors.Add("Hourly rate cannot exceed R500 per hour");

            // Only validate TotalAmount if it's set (non-zero)
            // The repository will calculate it if it's 0
            if (claim.TotalAmount > 0)
            {
                if (claim.TotalAmount <= 0)
                    errors.Add("Total amount must be greater than 0");
                else if (claim.TotalAmount > 50000)
                    errors.Add("Total amount cannot exceed R50,000 per claim");

                // Custom validation for business rules - only if TotalAmount is set
                var calculatedAmount = Math.Round(claim.HoursWorked * claim.HourlyRate, 2);
                if (Math.Abs(calculatedAmount - claim.TotalAmount) > 0.01)
                {
                    errors.Add($"Calculated amount (R{calculatedAmount:F2}) doesn't match provided total (R{claim.TotalAmount:F2})");
                }
            }

            if (claim.AdditionalNotes?.Length > 500)
                errors.Add("Additional notes cannot exceed 500 characters");

            return new ValidationResult
            {
                IsValid = errors.Count == 0,
                Errors = errors
            };
        }
    }

    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}