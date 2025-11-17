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
        private int _nextId = 1;

        public InMemoryClaimsRepository()
        {
            Seed();
        }

        public Task<Claim> CreateAsync(Claim claim)
        {
            var id = _nextId++;
            claim.Id = id;
            claim.TotalAmount = Math.Round(claim.HoursWorked * claim.HourlyRate, 2);
            claim.SubmittedAt = DateTime.UtcNow;
            claim.Status = "pending";
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
                ApprovedClaims = values.Count(c => c.Status == "approved"),
                RejectedClaims = values.Count(c => c.Status == "rejected"),
                TotalAmountApproved = Math.Round(values.Where(c => c.Status == "approved").Sum(c => c.TotalAmount), 2)
            };
            return Task.FromResult(summary);
        }

        private void Seed()
        {
            var now = DateTime.UtcNow;
            var s1 = new Claim { LecturerName = "Dr. Sarah Johnson", HoursWorked = 25.5, HourlyRate = 45.0, TotalAmount = 1147.5, AdditionalNotes = "Lecture prep", Status = "approved", SubmittedAt = now.AddDays(-2), ReviewedAt = now.AddDays(-1) };
            var s2 = new Claim { LecturerName = "Prof. Michael Chen", HoursWorked = 18, HourlyRate = 52, TotalAmount = 936, AdditionalNotes = "Tutorials", Status = "pending", SubmittedAt = now.AddHours(-6) };
            var s3 = new Claim { LecturerName = "Dr. Emily Watson", HoursWorked = 12, HourlyRate = 48, TotalAmount = 576, AdditionalNotes = "Lab supervision", Status = "rejected", SubmittedAt = now.AddDays(-3), ReviewedAt = now.AddDays(-2) };

            CreateAsync(s1).Wait();
            CreateAsync(s2).Wait();
            CreateAsync(s3).Wait();
        }
    }
}
