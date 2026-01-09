using JobPlatform.Application.Common.Interfaces;
using JobPlatform.Application.Skills.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.Application.Skills.Queries;

public sealed class GetSkillDomainsHandler : IRequestHandler<GetSkillDomainsQuery, List<SkillDomainDto>>
{
    private readonly IAppDbContext _db;
    public GetSkillDomainsHandler(IAppDbContext db) => _db = db;

    public async Task<List<SkillDomainDto>> Handle(GetSkillDomainsQuery request, CancellationToken ct)
    {
        return await _db.SkillDomains.AsNoTracking()
            .OrderBy(d => d.Code)
            .Select(d => new SkillDomainDto(d.Id, d.Code, d.Name))
            .ToListAsync(ct);
    }
}