using System.ComponentModel.DataAnnotations;

namespace API.Configuration;

public class RateLimitingOptions
{
    public const string SectionName = "RateLimiting";

    public AuthPolicy Auth { get; set; } = new();
    public AuthenticatedPolicy Authenticated { get; set; } = new();
    public AnonymousPolicy Anonymous { get; set; } = new();
    public GlobalPolicy Global { get; set; } = new();
    public ExpensiveOperationsPolicy ExpensiveOperations { get; set; } = new();

    public class AuthPolicy
    {
        [Range(1, 100, ErrorMessage = "Auth PermitLimit must be between 1 and 100.")]
        public int PermitLimit { get; set; } = 10;

        [Range(1, 60, ErrorMessage = "Auth WindowMinutes must be between 1 and 60.")]
        public int WindowMinutes { get; set; } = 1;
    }

    public class AuthenticatedPolicy
    {
        [Range(1, 1000, ErrorMessage = "Authenticated TokenCapacity must be between 1 and 1000.")]
        public int TokenCapacity { get; set; } = 100;

        [Range(1, 500, ErrorMessage = "Authenticated TokensPerMinute must be between 1 and 500.")]
        public int TokensPerMinute { get; set; } = 50;
    }

    public class AnonymousPolicy
    {
        [Range(1, 1000, ErrorMessage = "Anonymous PermitLimit must be between 1 and 1000.")]
        public int PermitLimit { get; set; } = 60;

        [Range(1, 60, ErrorMessage = "Anonymous WindowMinutes must be between 1 and 60.")]
        public int WindowMinutes { get; set; } = 1;

        [Range(1, 100, ErrorMessage = "Anonymous SegmentsPerWindow must be between 1 and 100.")]
        public int SegmentsPerWindow { get; set; } = 6;
    }

    public class GlobalPolicy
    {
        [Range(1, 10000, ErrorMessage = "Global PermitLimit must be between 1 and 10000.")]
        public int PermitLimit { get; set; } = 200;

        [Range(1, 60, ErrorMessage = "Global WindowMinutes must be between 1 and 60.")]
        public int WindowMinutes { get; set; } = 1;
    }

    public class ExpensiveOperationsPolicy
    {
        [Range(1, 100, ErrorMessage = "ExpensiveOperations PermitLimit must be between 1 and 100.")]
        public int PermitLimit { get; set; } = 20;

        [Range(1, 60, ErrorMessage = "ExpensiveOperations WindowMinutes must be between 1 and 60.")]
        public int WindowMinutes { get; set; } = 1;
    }
}