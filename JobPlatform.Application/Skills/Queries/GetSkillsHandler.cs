using JobPlatform.Application.Common.Interfaces;
using JobPlatform.Application.Skills.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.Application.Skills.Queries;

public sealed class GetSkillsHandler : IRequestHandler<GetSkillsQuery, List<SkillDto>>
{
    private readonly IAppDbContext _db;
    public GetSkillsHandler(IAppDbContext db) => _db = db;

    public async Task<List<SkillDto>> Handle(GetSkillsQuery request, CancellationToken ct)
    {
        var q = _db.Skills.AsNoTracking()
            .Include(s => s.Category).ThenInclude(c => c.Domain)
            .AsQueryable();

        if (request.ActiveOnly)
            q = q.Where(s => s.IsActive);

        if (!string.IsNullOrWhiteSpace(request.DomainCode))
        {
            var dc = request.DomainCode.Trim().ToUpperInvariant();
            q = q.Where(s => s.Category.Domain.Code == dc);
        }

        if (!string.IsNullOrWhiteSpace(request.CategoryCode))
        {
            var cc = request.CategoryCode.Trim().ToUpperInvariant();
            q = q.Where(s => s.Category.Code == cc);
        }

        if (!string.IsNullOrWhiteSpace(request.Q))
        {
            var keyword = request.Q.Trim();
            q = q.Where(s => EF.Functions.Like(s.Name, $"%{keyword}%") || EF.Functions.Like(s.Code, $"%{keyword}%"));
        }

        return await q.OrderBy(s => s.Name)
            .Select(s => new SkillDto(
                s.Id,
                s.CategoryId,
                s.Category.Domain.Code,
                s.Category.Code,
                s.Code,
                s.Name,
                s.IsActive
            ))
            .ToListAsync(ct);
    }
}