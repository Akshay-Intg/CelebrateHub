using CelebrateHub.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace CelebrateHub.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Employee> Employees => Set<Employee>();
        public DbSet<PartyEvent> PartyEvents => Set<PartyEvent>();
        public DbSet<PartyVote> PartyVotes => Set<PartyVote>();

        protected override void OnModelCreating(ModelBuilder mb)
        {
            base.OnModelCreating(mb);

            // ── Employee ──────────────────────────────────────────────────────
            mb.Entity<Employee>()
              .HasIndex(e => e.Email)
              .IsUnique();

            // ── PartyEvent relationships ──────────────────────────────────────
            mb.Entity<PartyEvent>()
              .HasOne(pe => pe.Employee)
              .WithMany()
              .HasForeignKey(pe => pe.EmployeeId)
              .OnDelete(DeleteBehavior.Cascade);

            mb.Entity<PartyEvent>()
              .HasIndex(pe => new { pe.EmployeeId, pe.EventType, pe.EventDate })
              .IsUnique();

            // ── PartyVote relationships ───────────────────────────────────────
            mb.Entity<PartyVote>()
              .HasOne(pv => pv.PartyEvent)
              .WithMany(pe => pe.Votes)
              .HasForeignKey(pv => pv.PartyEventId)
              .OnDelete(DeleteBehavior.Cascade);

            mb.Entity<PartyVote>()
              .HasOne(pv => pv.VoterEmployee)
              .WithMany()
              .HasForeignKey(pv => pv.VoterEmployeeId)
              .OnDelete(DeleteBehavior.NoAction);

            mb.Entity<PartyVote>()
              .HasIndex(pv => new { pv.PartyEventId, pv.VoterEmployeeId })
              .IsUnique();

            // ── Seed Employees — ALL values are static/hardcoded ─────────────
            mb.Entity<Employee>().HasData(
                new Employee
                {
                    EmployeeId = 1,
                    Name = "Admin User",
                    Email = "admin@portal.com",
                    // Pre-generated BCrypt hash of "Admin@123" — static string
                    PasswordHash = "$2a$11$CQsKloLgaA9GRRtcPnfxpee8lmuk.RCJhIdwg6CtLJ6vDN6iaL5.m",
                    DateOfBirth = new DateTime(1990, 6, 15),
                    AnniversaryDate = new DateTime(2018, 3, 10),
                    Department = "IT",
                    Role = "Admin",
                    CreatedDate = new DateTime(2024, 1, 1),
                    IsActive = true
                }
            );

            // ── Seed PartyEvents — static dates ──────────────────────────────
            mb.Entity<PartyEvent>().HasData(
                new PartyEvent
                {
                    PartyEventId = 1,
                    EmployeeId = 1,
                    EventType = "Birthday",
                    EventDate = new DateTime(2025, 6, 15),
                    Status = "Pending",
                    DonePercentage = 0,
                    TotalVotes = 0,
                    DoneVotes = 0,
                    PendingVotes = 0,
                    CreatedDate = new DateTime(2025, 6, 15),
                    IsActive = true
                },
                new PartyEvent
                {
                    PartyEventId = 2,
                    EmployeeId = 1,
                    EventType = "Anniversary",
                    EventDate = new DateTime(2025, 3, 10),
                    Status = "Pending",
                    DonePercentage = 0,
                    TotalVotes = 0,
                    DoneVotes = 0,
                    PendingVotes = 0,
                    CreatedDate = new DateTime(2025, 3, 10),
                    IsActive = true
                }
            );
        }
    }
}