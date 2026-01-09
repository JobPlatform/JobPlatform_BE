using JobPlatform.Domain.Entities;
using JobPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.Api.Seed;

public static class SkillCatalogSeeder
{
    public static async Task SeedSkillCatalogAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        
        if (await db.SkillDomains.AnyAsync()) return;

        await UpsertDomainAsync(
            db,
            domainCode: "IT",
            domainName: "Information Technology",
            categories: new[]
            {
                ("LANG", "Programming Languages", S(
                    ("C_SHARP","C#"),
                    ("JAVA","Java"),
                    ("JS","JavaScript"),
                    ("TS","TypeScript"),
                    ("PY","Python"),
                    ("GO","Go")
                )),
                ("FRAME", "Frameworks", S(
                    ("ASP_NET_CORE","ASP.NET Core"),
                    ("SPRING_BOOT","Spring Boot"),
                    ("NODEJS","Node.js")
                )),
                ("DB", "Databases", S(
                    ("SQL_SERVER","SQL Server"),
                    ("POSTGRES","PostgreSQL"),
                    ("MYSQL","MySQL"),
                    ("MONGODB","MongoDB"),
                    ("REDIS","Redis")
                )),
                ("DEVOPS", "DevOps", S(
                    ("DOCKER","Docker"),
                    ("K8S","Kubernetes"),
                    ("GITHUB_ACTIONS","GitHub Actions"),
                    ("AZURE_DEVOPS","Azure DevOps")
                )),
                ("CLOUD", "Cloud", S(
                    ("AWS","AWS"),
                    ("AZURE","Azure"),
                    ("GCP","Google Cloud")
                )),
            });

        await UpsertDomainAsync(
            db,
            domainCode: "DATA",
            domainName: "Data & Analytics",
            categories: new[]
            {
                ("ANALYTICS", "Analytics", S(
                    ("POWER_BI","Power BI"),
                    ("TABLEAU","Tableau"),
                    ("EXCEL","Excel")
                )),
                ("DATA_ENG", "Data Engineering", S(
                    ("AIRFLOW","Apache Airflow"),
                    ("SPARK","Apache Spark"),
                    ("DBT","dbt")
                )),
                ("ML", "Machine Learning", S(
                    ("SKLEARN","scikit-learn"),
                    ("PYTORCH","PyTorch"),
                    ("TENSORFLOW","TensorFlow")
                )),
            });

        await UpsertDomainAsync(
            db,
            domainCode: "DESIGN",
            domainName: "Design",
            categories: new[]
            {
                ("UIUX", "UI/UX", S(
                    ("FIGMA","Figma"),
                    ("WIREFRAME","Wireframing"),
                    ("DESIGN_SYSTEM","Design System")
                )),
                ("GRAPHIC", "Graphic Design", S(
                    ("PHOTOSHOP","Adobe Photoshop"),
                    ("ILLUSTRATOR","Adobe Illustrator")
                )),
            });

        await UpsertDomainAsync(
            db,
            domainCode: "BUS",
            domainName: "Business & Sales",
            categories: new[]
            {
                ("SALES", "Sales", S(
                    ("B2B_SALES","B2B Sales"),
                    ("NEGOTIATION","Negotiation"),
                    ("CRM","CRM")
                )),
                ("MARKETING", "Marketing", S(
                    ("SEO","SEO"),
                    ("CONTENT","Content Marketing"),
                    ("ADS","Paid Ads")
                )),
            });

        await db.SaveChangesAsync();
    }

    
    private static IEnumerable<(string skillCode, string skillName)> S(
        params (string skillCode, string skillName)[] items
    ) => items;

    private static async Task UpsertDomainAsync(
        AppDbContext db,
        string domainCode,
        string domainName,
        IEnumerable<(string categoryCode, string categoryName, IEnumerable<(string skillCode, string skillName)> skills)> categories)
    {
        domainCode = domainCode.Trim().ToUpperInvariant();

        
        var domain = await db.SkillDomains
            .FirstOrDefaultAsync(d => d.Code == domainCode);

        if (domain is null)
        {
            domain = new SkillDomain
            {
                Code = domainCode,
                Name = domainName
            };
            db.SkillDomains.Add(domain);
            await db.SaveChangesAsync(); // cần để có domain.Id
        }
        else if (domain.Name != domainName)
        {
            domain.Name = domainName;
        }

        
        var existingCategories = await db.SkillCategories
            .Where(c => c.DomainId == domain.Id)
            .ToListAsync();

        foreach (var (categoryCodeRaw, categoryName, skills) in categories)
        {
            var categoryCode = categoryCodeRaw.Trim().ToUpperInvariant();

            var category = existingCategories.FirstOrDefault(c => c.Code == categoryCode);

            if (category is null)
            {
                category = new SkillCategory
                {
                    DomainId = domain.Id,
                    Code = categoryCode,
                    Name = categoryName
                };
                db.SkillCategories.Add(category);
                existingCategories.Add(category);
                await db.SaveChangesAsync(); // cần để có category.Id
            }
            else if (category.Name != categoryName)
            {
                category.Name = categoryName;
            }

            // preload existing skills for performance (by code)
            var skillCodes = skills
                .Select(s => s.skillCode?.Trim().ToUpperInvariant())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToList()!;

            if (skillCodes.Count == 0) continue;

            var existingSkills = await db.Skills
                .Where(s => skillCodes.Contains(s.Code))
                .ToListAsync();

            foreach (var (skillCodeRaw, skillNameRaw) in skills)
            {
                var skillCode = skillCodeRaw.Trim().ToUpperInvariant();
                if (string.IsNullOrWhiteSpace(skillCode)) continue;

                var skillName = (skillNameRaw ?? "").Trim();
                if (string.IsNullOrWhiteSpace(skillName)) continue;

               
                var skill = existingSkills.FirstOrDefault(s => s.Code == skillCode);

                if (skill is null)
                {
                    db.Skills.Add(new Skill
                    {
                        CategoryId = category.Id,
                        Code = skillCode,
                        Name = skillName,
                        IsActive = true
                    });
                }
                else
                {
                    
                    skill.CategoryId = category.Id;
                    skill.Name = skillName;
                    if (!skill.IsActive) skill.IsActive = true;
                }
            }
        }
    }
}
