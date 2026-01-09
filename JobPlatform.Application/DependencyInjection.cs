using System.Reflection;
using JobPlatform.Application.Common.Pipeline;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace JobPlatform.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Đăng ký MediatR
        services.AddMediatR(Assembly.GetExecutingAssembly());

        // Đăng ký AutoMapper
        services.AddAutoMapper(cfg => { }, Assembly.GetExecutingAssembly());
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));


        return services;
    }
}