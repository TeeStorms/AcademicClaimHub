using ClaimManagementHub.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace ClaimManagementHub.Services
{
    public class InMemoryUserService
    {
        private readonly List<ApplicationUser> _users = new();
        private readonly PasswordHasher<ApplicationUser> _passwordHasher = new();

        public InMemoryUserService()
        {
            SeedUsers();
        }

        public ApplicationUser? FindByUsername(string username)
        {
            return _users.FirstOrDefault(u => u.UserName == username);
        }

        public bool ValidatePassword(ApplicationUser user, string password)
        {
            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
            return result == PasswordVerificationResult.Success;
        }

        public List<ApplicationUser> GetAllUsers() => _users;

        private void SeedUsers()
        {
            // Lecturer
            var lecturer = new ApplicationUser
            {
                Id = "1",
                UserName = "lecturer@university.com",
                Email = "lecturer@university.com",
                FullName = "Dr. Sarah Johnson",
                Role = "Lecturer"
            };
            lecturer.PasswordHash = _passwordHasher.HashPassword(lecturer, "Password123!");
            _users.Add(lecturer);

            // Coordinator
            var coordinator = new ApplicationUser
            {
                Id = "2",
                UserName = "coordinator@university.com",
                Email = "coordinator@university.com",
                FullName = "Prof. Michael Chen",
                Role = "Coordinator"
            };
            coordinator.PasswordHash = _passwordHasher.HashPassword(coordinator, "Password123!");
            _users.Add(coordinator);

            // HR
            var hr = new ApplicationUser
            {
                Id = "3",
                UserName = "hr@university.com",
                Email = "hr@university.com",
                FullName = "Emma Wilson",
                Role = "HR"
            };
            hr.PasswordHash = _passwordHasher.HashPassword(hr, "Password123!");
            _users.Add(hr);

            // Academic Manager
            var manager = new ApplicationUser
            {
                Id = "4",
                UserName = "manager@university.com",
                Email = "manager@university.com",
                FullName = "Dr. James Brown",
                Role = "Manager"
            };
            manager.PasswordHash = _passwordHasher.HashPassword(manager, "Password123!");
            _users.Add(manager);
        }
    }
}