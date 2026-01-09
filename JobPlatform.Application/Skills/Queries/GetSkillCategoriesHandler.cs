using JobPlatform.Application.Common.Interfaces;
using JobPlatform.Application.Skills.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.Application.Skills.Queries;

public sealed class GetSkillCategoriesHandler : IRequestHandler<GetSkillCategoriesQuery, List<SkillCategoryDto>>
{
    private readonly IAppDbContext _db;
    public GetSkillCategoriesHandler(IAppDbContext db) => _db = db;

    public async Task<List<SkillCategoryDto>> Handle(GetSkillCategoriesQuery request, CancellationToken ct)
    {
        var q = _db.SkillCategories.AsNoTracking().Include(c => c.Domain).AsQueryable();

        if (request.DomainId.HasValue)
            q = q.Where(c => c.DomainId == request.DomainId.Value);

        if (!string.IsNullOrWhiteSpace(request.DomainCode))
        {
            var code = request.DomainCode.Trim().ToUpperInvariant();
            q = q.Where(c => c.Domain.Code == code);
        }

        return await q.OrderBy(c => c.Code)
            .Select(c => new SkillCategoryDto(c.Id, c.DomainId, c.Domain.Code, c.Code, c.Name))
            .ToListAsync(ct);
    }
}