using System.Collections.Generic;
using AcademicClaimHub.Models;

namespace AcademicClaimHub.Data
{
    // Static repository class to store and manage Claim objects in memory
    public static class ClaimRepository
    {
        // In-memory list that holds all claims
        public static List<Claim> Claims { get; } = new List<Claim>();

        // Method to add a new claim to the repository
        public static void AddClaim(Claim claim)
        {
            Claims.Add(claim);
        }
    }
}
