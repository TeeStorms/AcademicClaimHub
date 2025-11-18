using ClaimManagementHub.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClaimManagementHub.Services
{
    public class InMemoryClaimsRepository : IClaimsRepository
    {
        private readonly ConcurrentDictionary<int, Claim> _store = new();
        private readonly ApprovalWorkflowService _workflowService;
        private readonly ClaimValidator _validator;
        private int _nextId = 1;

        public InMemoryClaimsRepository()
        {
            _workflowService = new ApprovalWorkflowService();
            _validator = new ClaimValidator();
            Seed();
        }

        public Task<Claim> CreateAsync(Claim claim)
        {
            // Validate claim
            var validationResult = _validator.Validate(claim);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new InvalidOperationException($"Claim validation failed: {errors}");
            }

            var id = _nextId++;
            claim.Id = id;
            claim.TotalAmount = Math.Round(claim.HoursWorked * claim.HourlyRate, 2);
            claim.SubmittedAt = DateTime.UtcNow;
            claim.Status = "pending";

            // Process through approval workflow
            var workflowResult = _workflowService.ProcessClaim(claim);

            if (workflowResult.IsAutoApproved)
            {
                claim.ReviewedAt = DateTime.UtcNow;
            }

            _store[id] = claim;
            return Task.FromResult(claim);
        }

        public Task<IEnumerable<Claim>> GetAllAsync()
        {
            var list = _store.Values.OrderByDescending(c => c.SubmittedAt).ToList();
            return Task.FromResult<IEnumerable<Claim>>(list);
        }

        public Task<Claim?> GetByIdAsync(int id)
        {
            _store.TryGetValue(id, out var claim);
            return Task.FromResult(claim);
        }

        public Task<Claim?> UpdateStatusAsync(int id, string status)
        {
            if (!_store.TryGetValue(id, out var claim)) return Task.FromResult<Claim?>(null);

            claim.Status = status;
            claim.ReviewedAt = DateTime.UtcNow;
            _store[id] = claim;

            return Task.FromResult<Claim?>(claim);
        }

        public Task<IEnumerable<Claim>> GetRecentAsync(int count)
        {
            var list = _store.Values.OrderByDescending(c => c.SubmittedAt).Take(count).ToList();
            return Task.FromResult<IEnumerable<Claim>>(list);
        }

        public Task<ClaimSummary> GetSummaryAsync()
        {
            var values = _store.Values.ToList();
            var summary = new ClaimSummary
            {
                TotalClaims = values.Count,
                PendingClaims = values.Count(c => c.Status == "pending"),
                ApprovedClaims = values.Count(c => c.Status == "approved" || c.Status == "auto-approved"),
                RejectedClaims = values.Count(c => c.Status == "rejected"),
                TotalAmountApproved = Math.Round(values.Where(c => c.Status == "approved" || c.Status == "auto-approved").Sum(c => c.TotalAmount), 2),
                AutoApprovedClaims = values.Count(c => c.Status == "auto-approved")
            };
            return Task.FromResult(summary);
        }

        // New method for workflow analysis
        public Task<WorkflowAnalysis> GetWorkflowAnalysisAsync()
        {
            var claims = _store.Values.ToList();
            var analysis = new WorkflowAnalysis
            {
                TotalClaims = claims.Count,
                AutoApprovedCount = claims.Count(c => c.Status == "auto-approved"),
                FlaggedClaims = claims.Count(c => c.AdditionalNotes?.Contains("[FLAGGED:") == true),
                AverageProcessingTime = claims
                    .Where(c => c.ReviewedAt.HasValue)
                    .Average(c => (c.ReviewedAt.Value - c.SubmittedAt).TotalHours),
                CommonFlags = claims
                    .SelectMany(c => _workflowService.GetMatchingRules(c).Select(r => r.Name))
                    .GroupBy(name => name)
                    .ToDictionary(g => g.Key, g => g.Count())
            };

            return Task.FromResult(analysis);
        }

        private void Seed()
        {
            var now = DateTime.UtcNow;

            // Regular claims
            var s1 = new Claim { LecturerName = "Dr. Sarah Johnson", HoursWorked = 25.5, HourlyRate = 45.0, TotalAmount = 1147.5, AdditionalNotes = "Lecture prep", Status = "approved", SubmittedAt = now.AddDays(-2), ReviewedAt = now.AddDays(-1) };
            var s2 = new Claim { LecturerName = "Prof. Michael Chen", HoursWorked = 18, HourlyRate = 52, TotalAmount = 936, AdditionalNotes = "Tutorials", Status = "pending", SubmittedAt = now.AddHours(-6) };
            var s3 = new Claim { LecturerName = "Dr. Emily Watson", HoursWorked = 12, HourlyRate = 48, TotalAmount = 576, AdditionalNotes = "Lab supervision", Status = "rejected", SubmittedAt = now.AddDays(-3), ReviewedAt = now.AddDays(-2) };

            // Auto-approval examples
            var s4 = new Claim { LecturerName = "Dr. James Wilson", HoursWorked = 8, HourlyRate = 45, TotalAmount = 360, AdditionalNotes = "Guest lecture", Status = "auto-approved", SubmittedAt = now.AddHours(-2), ReviewedAt = now.AddHours(-1) };
            var s5 = new Claim { LecturerName = "Prof. Lisa Brown", HoursWorked = 5, HourlyRate = 50, TotalAmount = 250, AdditionalNotes = "Consultation", Status = "auto-approved", SubmittedAt = now.AddHours(-1), ReviewedAt = now.AddMinutes(-30) };

            CreateAsync(s1).Wait();
            CreateAsync(s2).Wait();
            CreateAsync(s3).Wait();
            CreateAsync(s4).Wait();
            CreateAsync(s5).Wait();
        }
    }

    public class WorkflowAnalysis
    {
        public int TotalClaims { get; set; }
        public int AutoApprovedCount { get; set; }
        public int FlaggedClaims { get; set; }
        public double AverageProcessingTime { get; set; }
        public Dictionary<string, int> CommonFlags { get; set; } = new();
    }
}