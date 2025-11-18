using ClaimManagementHub.Models;
using FluentValidation;

namespace ClaimManagementHub.Services
{
    public class ClaimValidator : AbstractValidator<Claim>
    {
        public ClaimValidator()
        {
            RuleFor(x => x.LecturerName)
                .NotEmpty().WithMessage("Lecturer name is required")
                .Length(2, 100).WithMessage("Lecturer name must be between 2 and 100 characters");

            RuleFor(x => x.HoursWorked)
                .GreaterThan(0).WithMessage("Hours worked must be greater than 0")
                .LessThanOrEqualTo(100).WithMessage("Hours worked cannot exceed 100 hours per claim")
                .ScalePrecision(2, 5).WithMessage("Hours worked can have maximum 2 decimal places");

            RuleFor(x => x.HourlyRate)
                .GreaterThan(0).WithMessage("Hourly rate must be greater than 0")
                .LessThanOrEqualTo(500).WithMessage("Hourly rate cannot exceed R500 per hour")
                .ScalePrecision(2, 6).WithMessage("Hourly rate can have maximum 2 decimal places");

            RuleFor(x => x.TotalAmount)
                .GreaterThan(0).WithMessage("Total amount must be greater than 0")
                .LessThanOrEqualTo(50000).WithMessage("Total amount cannot exceed R50,000 per claim");

            RuleFor(x => x.AdditionalNotes)
                .MaximumLength(500).WithMessage("Additional notes cannot exceed 500 characters");

            // Custom validation for business rules
            RuleFor(x => x).Custom((claim, context) =>
            {
                var calculatedAmount = Math.Round(claim.HoursWorked * claim.HourlyRate, 2);
                if (Math.Abs(calculatedAmount - claim.TotalAmount) > 0.01)
                {
                    context.AddFailure("TotalAmount", $"Calculated amount (R{calculatedAmount:F2}) doesn't match provided total (R{claim.TotalAmount:F2})");
                }

                // Auto-approval for small amounts
                if (claim.TotalAmount <= 1000 && claim.HoursWorked <= 10)
                {
                    claim.Status = "auto-approved";
                }
            });
        }
    }
}