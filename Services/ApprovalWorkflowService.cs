using ClaimManagementHub.Models;

namespace ClaimManagementHub.Services
{
    public class ApprovalWorkflowService
    {
        private readonly List<ApprovalRule> _rules;

        public ApprovalWorkflowService()
        {
            _rules = new()
            {
                new()
                {
                    Name = "Small Claim Auto-Approval",
                    Condition = claim => claim.TotalAmount <= 1000 && claim.HoursWorked <= 10,
                    Action = claim => {
                        claim.Status = "approved";
                        claim.AdditionalNotes += " [AUTO-APPROVED: Small claim]";
                    }
                },
                new()
                {
                    Name = "High Amount Flag",
                    Condition = claim => claim.TotalAmount > 5000,
                    Action = claim => {
                        claim.AdditionalNotes += " [FLAGGED: High amount requires manager review]";
                    }
                },
                new()
                {
                    Name = "Overtime Check",
                    Condition = claim => claim.HoursWorked > 40,
                    Action = claim => {
                        claim.AdditionalNotes += " [FLAGGED: Overtime hours require justification]";
                    }
                },
                new()
                {
                    Name = "Unusual Rate Check",
                    Condition = claim => claim.HourlyRate > 200 || claim.HourlyRate < 30,
                    Action = claim => {
                        claim.AdditionalNotes += " [FLAGGED: Unusual hourly rate requires verification]";
                    }
                }
            };
        }

        public ApprovalResult ProcessClaim(Claim claim)
        {
            var result = new ApprovalResult { Claim = claim };
            var flags = new List<string>();

            foreach (var rule in _rules)
            {
                if (rule.Condition(claim))
                {
                    rule.Action(claim);
                    flags.Add(rule.Name);
                }
            }

            result.Flags = flags;
            result.IsAutoApproved = claim.Status == "approved";

            return result;
        }

        public List<ApprovalRule> GetMatchingRules(Claim claim)
        {
            return _rules.Where(rule => rule.Condition(claim)).ToList();
        }
    }

    public class ApprovalRule
    {
        public string Name { get; set; } = string.Empty;
        public Func<Claim, bool> Condition { get; set; } = _ => false;
        public Action<Claim> Action { get; set; } = _ => { };
    }

    public class ApprovalResult
    {
        public Claim Claim { get; set; } = new();
        public List<string> Flags { get; set; } = new();
        public bool IsAutoApproved { get; set; }
        public string Status => IsAutoApproved ? "Auto-Approved" : "Requires Review";
    }
}