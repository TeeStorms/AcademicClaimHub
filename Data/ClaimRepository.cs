using System.Collections.Generic;
using AcademicClaimHub.Models;

namespace AcademicClaimHub.Data
{
    public static class ClaimRepository
    {
        public static List<Claim> Claims { get; } = new List<Claim>();

        public static void AddClaim(Claim claim)
        {
            Claims.Add(claim);
        }
    }
}
