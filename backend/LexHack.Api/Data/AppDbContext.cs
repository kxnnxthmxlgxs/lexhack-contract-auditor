using Microsoft.EntityFrameworkCore;

namespace LexHack.Api.Data
{
    public class Playbook
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<PlaybookRule> Rules { get; set; } = new();
    }

    public class PlaybookRule
    {
        public int Id { get; set; }
        public int PlaybookId { get; set; }
        public string ClauseType { get; set; } = string.Empty;
        public string Operator { get; set; } = string.Empty; 
        public string TargetValue { get; set; } = string.Empty;
        public string Severity { get; set; } = "Warning"; 
    }

    public class AuditRun
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
        public int RiskScore { get; set; }
        public List<AuditResult> Results { get; set; } = new();
    }

    public class AuditResult
    {
        public int Id { get; set; }
        public int AuditRunId { get; set; }
        public string ClauseType { get; set; } = string.Empty;
        public string ExtractedValue { get; set; } = string.Empty;
        public bool Passed { get; set; }
    }

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Playbook> Playbooks => Set<Playbook>();
        public DbSet<PlaybookRule> PlaybookRules => Set<PlaybookRule>();
        public DbSet<AuditRun> AuditRuns => Set<AuditRun>();
        public DbSet<AuditResult> AuditResults => Set<AuditResult>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Seed the Playbook
            modelBuilder.Entity<Playbook>().HasData(
                new Playbook { Id = 1, Name = "Standard Vendor NDA" }
            );

            // 2. Seed the Rules for that Playbook
            modelBuilder.Entity<PlaybookRule>().HasData(
                new PlaybookRule { Id = 1, PlaybookId = 1, ClauseType = "governing_law", Operator = "Equals", TargetValue = "South Africa", Severity = "Critical" },
                new PlaybookRule { Id = 2, PlaybookId = 1, ClauseType = "liability_cap_amount", Operator = "LessThanOrEqual", TargetValue = "500000", Severity = "Warning" },
                new PlaybookRule { Id = 3, PlaybookId = 1, ClauseType = "is_indemnification_mutual", Operator = "Equals", TargetValue = "true", Severity = "Critical" },
                new PlaybookRule { Id = 4, PlaybookId = 1, ClauseType = "non_solicitation_months", Operator = "LessThanOrEqual", TargetValue = "12", Severity = "Warning" },
                new PlaybookRule { Id = 5, PlaybookId = 1, ClauseType = "unilateral_termination_days", Operator = "GreaterThanOrEqual", TargetValue = "30", Severity = "Critical" }
            );
        }
    }
}