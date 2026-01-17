using JobPlatform.Domain.Entities.Jobs;
using JobPlatform.Domain.Entities.Profiles;
using JobPlatform.Infrastructure.Persistence;

namespace JobPlatform.Api.Services;

using System.Text.Json;
using JobPlatform.Application.Common.Interfaces;
using JobPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;

public sealed class JobMatchingService : IJobMatchingService
{
    private readonly AppDbContext _db;
    private readonly INotificationPublisher _noti;

    public JobMatchingService(AppDbContext db, INotificationPublisher noti)
    {
        _db = db;
        _noti = noti;
    }

    public async Task MatchForJobAsync(Guid jobId, CancellationToken ct)
    {
        var job = await _db.JobPosts
            .Include(j => j.SkillRequirements)
                .ThenInclude(r => r.Skill)
                    .ThenInclude(s => s.Category)
                        .ThenInclude(c => c.Domain)
            .AsNoTracking()
            .FirstOrDefaultAsync(j => j.Id == jobId, ct);
        if (job is null || job.Status != JobStatus.Published) return;

        var reqs = job.SkillRequirements.ToList();
        if (reqs.Count == 0) return;

        
        var must = reqs.Where(r => r.IsMustHave).ToList();
        var allSkillIds = reqs.Select(r => r.SkillId).Distinct().ToList();

        
        IQueryable<CandidateProfile> q = _db.CandidateProfiles.AsNoTracking();

        
        foreach (var r in must)
        {
            var rid = r.SkillId;
            var lvl = r.RequiredLevel;
            q = q.Where(p => p.Skills.Any(cs => cs.SkillId == rid && cs.Level >= lvl));
        }

        

        var jobDomainIds = reqs
            .Select(r => r.Skill.Category.DomainId)
            .Distinct()
            .ToHashSet();
        
        var candidates = await q
            .Include(p => p.Skills.Where(cs => allSkillIds.Contains(cs.SkillId)))
                .ThenInclude(cs => cs.Skill)
                    .ThenInclude(s => s.Category)
                        .ThenInclude(c => c.Domain)
            .Take(2000)
            .ToListAsync(ct);

        candidates = candidates
            .Where(p => p.Skills.Any(cs => cs.Skill.Category.DomainId != Guid.Empty && jobDomainIds.Contains(cs.Skill.Category.DomainId)))
            .ToList();
        
        var scored = candidates
            .Select(p => new
            {
                Profile = p,
                Score = ComputeScore(p, job, reqs)
            })
            .Where(x => x.Score >= 1m)   
            .OrderByDescending(x => x.Score)
            .Take(200)                  
            .ToList();

        if (scored.Count == 0) return;

        var candidateIds = scored.Select(x => x.Profile.Id).ToList();

        
        var existing = await _db.JobMatches
            .Where(m => m.JobPostId == jobId && candidateIds.Contains(m.CandidateProfileId))
            .ToListAsync(ct);

        var existingMap = existing.ToDictionary(x => x.CandidateProfileId, x => x);

        foreach (var item in scored)
        {
            if (!existingMap.TryGetValue(item.Profile.Id, out var match))
            {
                match = new JobMatch
                {
                    JobPostId = jobId,
                    CandidateProfileId = item.Profile.Id,
                    Score = item.Score,
                    IsNotified = false
                };
                _db.JobMatches.Add(match);
                existingMap[item.Profile.Id] = match;
            }
            else
            {
                match.Score = item.Score;
                match.UpdatedAt = DateTimeOffset.UtcNow;
            }
        }

        await _db.SaveChangesAsync(ct);

        
        var toNotify = existingMap.Values
            .Where(m => !m.IsNotified)
            .OrderByDescending(m => m.Score)
            .Take(50) 
            .ToList();

        foreach (var m in toNotify)
        {
            var profile = scored.First(x => x.Profile.Id == m.CandidateProfileId).Profile;

            await _noti.NotifyAsync(
                userId: profile.UserId,
                type: "JobMatched",
                title: $"New job matches you: {job.Title}",
                body: $"We found a job matching your skills. Score: {m.Score:0.##}",
                data: new { jobId = job.Id, score = m.Score },
                targetUrl: $"/jobs/{job.Id}",
                sendEmail: true, 
                ct: ct);

            m.IsNotified = true;
            m.NotifiedAt = DateTimeOffset.UtcNow;
        }

        await _db.SaveChangesAsync(ct);
    }

    private static decimal ComputeScore(CandidateProfile p, JobPost job, List<JobSkillRequirement> reqs)
{
    decimal score = 0m;

  
    foreach (var r in reqs)
    {
        var cs = p.Skills.FirstOrDefault(x => x.SkillId == r.SkillId);
        if (cs is null)
        {
            
            continue;
        }

        
        var levelRatio = Math.Clamp(cs.Level / 5m, 0m, 1m);
        var requiredRatio = Math.Clamp(r.RequiredLevel / 5m, 0m, 1m);

        
        var meets = cs.Level >= r.RequiredLevel;
        var levelScore = meets ? levelRatio : levelRatio * 0.3m;

        // years bonus max +20%
        var yearsBonus = Math.Min(cs.Years / 5m, 1m) * 0.2m;

        var baseWeight = (decimal)Math.Max(1, r.Weight);
        var w = r.IsMustHave ? baseWeight * 2.5m : baseWeight; // must-have mạnh hơn

        score += w * (levelScore + yearsBonus) * (0.8m + 0.2m * requiredRatio);
    }

    
    var jobDomainIds = reqs.Select(r => r.Skill.Category.DomainId).Distinct().ToList();
    var jobCategoryIds = reqs.Select(r => r.Skill.CategoryId).Distinct().ToList();

    var candDomainIds = p.Skills.Select(cs => cs.Skill.Category.DomainId).Distinct().ToList();
    var candCategoryIds = p.Skills.Select(cs => cs.Skill.CategoryId).Distinct().ToList();

    var domainOverlap = jobDomainIds.Intersect(candDomainIds).Count();
    var categoryOverlap = jobCategoryIds.Intersect(candCategoryIds).Count();

    if (domainOverlap > 0) score += 1.0m;                        
    score += 0.3m * categoryOverlap;                              

   
    if (p.PreferRemote && job.WorkMode == WorkMode.Remote) score += 0.8m;

    
    if (p.ExpectedSalaryMin.HasValue && job.SalaryMax.HasValue && p.ExpectedSalaryMin.Value <= job.SalaryMax.Value)
        score += 0.5m;

    return score;
}

}
