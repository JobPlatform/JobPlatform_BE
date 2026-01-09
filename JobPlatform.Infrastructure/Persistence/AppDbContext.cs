using JobPlatform.Application.Common.Interfaces;
using JobPlatform.Domain.Entities;
using JobPlatform.Domain.Entities.Applications;
using JobPlatform.Domain.Entities.Chats;
using JobPlatform.Domain.Entities.Interviews;
using JobPlatform.Domain.Entities.Jobs;
using JobPlatform.Domain.Entities.Notifications;
using JobPlatform.Domain.Entities.Profiles;
using JobPlatform.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<SkillDomain> SkillDomains => Set<SkillDomain>();
    public DbSet<SkillCategory> SkillCategories => Set<SkillCategory>();
    public DbSet<Skill> Skills => Set<Skill>();

    public DbSet<CandidateProfile> CandidateProfiles => Set<CandidateProfile>();
    public DbSet<EmployerProfile> EmployerProfiles => Set<EmployerProfile>();
    public DbSet<JobPost> JobPosts => Set<JobPost>();
    public DbSet<JobApplication> JobApplications => Set<JobApplication>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Interview> Interviews => Set<Interview>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Skill
        modelBuilder.Entity<SkillDomain>()
            .HasIndex(x => x.Code).IsUnique();

        modelBuilder.Entity<SkillCategory>()
            .HasIndex(x => new { x.DomainId, x.Code }).IsUnique();

        modelBuilder.Entity<Skill>()
            .HasIndex(x => x.Code).IsUnique();

        modelBuilder.Entity<Skill>()
            .HasIndex(x => new { x.CategoryId, x.Name }).IsUnique();

        modelBuilder.Entity<JobSkillRequirement>()
            .Property(x => x.RequirementDescription)
            .HasMaxLength(500);

        // CandidateProfile: 1 user -> 1 profile
        modelBuilder.Entity<CandidateProfile>()
            .HasIndex(x => x.UserId)
            .IsUnique();

        // EmployerProfile: 1 user -> 1 profile
        modelBuilder.Entity<EmployerProfile>()
            .HasIndex(x => x.UserId)
            .IsUnique();

        // CandidateSkill composite key
        modelBuilder.Entity<CandidateSkill>()
            .HasKey(x => new { x.CandidateProfileId, x.SkillId });

        modelBuilder.Entity<CandidateSkill>()
            .HasOne(x => x.CandidateProfile)
            .WithMany(x => x.Skills)
            .HasForeignKey(x => x.CandidateProfileId);

        modelBuilder.Entity<CandidateSkill>()
            .HasOne(x => x.Skill)
            .WithMany()
            .HasForeignKey(x => x.SkillId);
        modelBuilder.Entity<CandidateSkill>()
            .Property(x => x.Years)
            .HasColumnType("decimal(4,1)"); // 0.0 -> 99.9

        modelBuilder.Entity<CandidateProfile>()
            .Property(x => x.CvFileName)
            .HasMaxLength(255);

        modelBuilder.Entity<CandidateProfile>()
            .Property(x => x.CvContentType)
            .HasMaxLength(100);

        modelBuilder.Entity<CandidateProfile>()
            .Property(x => x.CvStoragePath)
            .HasMaxLength(500);
        
        // JobSkillRequirement composite key
        modelBuilder.Entity<JobSkillRequirement>()
            .HasKey(x => new { x.JobPostId, x.SkillId });

        modelBuilder.Entity<JobSkillRequirement>()
            .HasOne(x => x.JobPost)
            .WithMany(x => x.SkillRequirements)
            .HasForeignKey(x => x.JobPostId);

        modelBuilder.Entity<JobSkillRequirement>()
            .HasOne(x => x.Skill)
            .WithMany()
            .HasForeignKey(x => x.SkillId);

        // JobPost -> EmployerProfile
        modelBuilder.Entity<JobPost>()
            .HasOne(x => x.EmployerProfile)
            .WithMany()
            .HasForeignKey(x => x.EmployerProfileId);

        // JobApplication unique: 1 candidate apply 1 job only once
        modelBuilder.Entity<JobApplication>()
            .HasIndex(x => new { x.JobPostId, x.CandidateProfileId })
            .IsUnique();

        // Conversation: 1 application -> 1 conversation (tuỳ bạn, mình set unique)
        modelBuilder.Entity<Conversation>()
            .HasIndex(x => x.JobApplicationId)
            .IsUnique();

        // ConversationMember composite
        modelBuilder.Entity<ConversationMember>()
            .HasKey(x => new { x.ConversationId, x.UserId });

        modelBuilder.Entity<ConversationMember>()
            .HasOne(x => x.Conversation)
            .WithMany(x => x.Members)
            .HasForeignKey(x => x.ConversationId);

        // Message
        modelBuilder.Entity<Message>()
            .HasOne(x => x.Conversation)
            .WithMany(x => x.Messages)
            .HasForeignKey(x => x.ConversationId);

        // InterviewParticipant composite
        modelBuilder.Entity<InterviewParticipant>()
            .HasKey(x => new { x.InterviewId, x.UserId });

        modelBuilder.Entity<InterviewParticipant>()
            .HasOne(x => x.Interview)
            .WithMany(x => x.Participants)
            .HasForeignKey(x => x.InterviewId);

        // Recommendation composite
        modelBuilder.Entity<JobRecommendation>()
            .HasKey(x => new { x.JobPostId, x.CandidateProfileId });

        modelBuilder.Entity<JobRecommendation>()
            .HasOne(x => x.JobPost)
            .WithMany()
            .HasForeignKey(x => x.JobPostId);

        modelBuilder.Entity<JobRecommendation>()
            .HasOne(x => x.CandidateProfile)
            .WithMany()
            .HasForeignKey(x => x.CandidateProfileId);
    }
}
