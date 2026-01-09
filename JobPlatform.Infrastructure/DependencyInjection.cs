using System.Reflection;
using JobPlatform.Application.Common.Interfaces;
using JobPlatform.Application.Common.Pipeline;
using JobPlatform.Infrastructure.Identity;
using JobPlatform.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JobPlatform.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var conn = config.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(conn))
            throw new InvalidOperationException("Missing ConnectionStrings:DefaultConnection");

        services.AddDbContext<AppDbContext>(opt => opt.UseSqlServer(conn));

        services
            .AddIdentity<ApplicationUser, IdentityRole<Guid>>(opt =>
            {
                opt.User.RequireUniqueEmail = true;
                opt.Password.RequiredLength = 6;
                opt.Password.RequireDigit = false;
                opt.Password.RequireNonAlphanumeric = false;
                opt.Password.RequireUppercase = false;
                opt.Password.RequireLowercase = false;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();
        
        services.AddScoped<ITokenService, TokenService>();

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<DbContext, AppDbContext>();
        return services;
    }
    
    
}