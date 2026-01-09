using JobPlatform.Domain.Entities;
using JobPlatform.Domain.Entities.Applications;
using JobPlatform.Domain.Entities.Chats;
using JobPlatform.Domain.Entities.Interviews;
using JobPlatform.Domain.Entities.Jobs;
using JobPlatform.Domain.Entities.Notifications;
using JobPlatform.Domain.Entities.Profiles;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.Application.Common.Interfaces;

public interface IAppDbContext
{
    DbSet<SkillDomain> SkillDomains { get; }
    DbSet<SkillCategory> SkillCategories { get; }
    DbSet<Skill> Skills { get; }

    DbSet<CandidateProfile> CandidateProfiles { get; }
    DbSet<EmployerProfile> EmployerProfiles { get; }

    DbSet<JobPost> JobPosts { get; }
    DbSet<JobApplication> JobApplications { get; }

    DbSet<Conversation> Conversations { get; }
    DbSet<Message> Messages { get; }

    DbSet<Interview> Interviews { get; }

    DbSet<Notification> Notifications { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}