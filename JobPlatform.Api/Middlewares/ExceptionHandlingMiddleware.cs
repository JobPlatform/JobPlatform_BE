using System.ComponentModel.DataAnnotations;
using System.Net;
using FluentValidation;
using JobPlatform.Application.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace JobPlatform.Api.Middlewares;

public sealed class ExceptionHandlingMiddleware : IMiddleware
{
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger) => _logger = logger;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            var traceId = context.TraceIdentifier;

            // Map status
            var (status, problem) = ex switch
            {
                FluentValidation.ValidationException vex => (
                    StatusCodes.Status400BadRequest,
                    BuildValidationProblem(context, vex, traceId)
                ),

                BadRequestException bre => (
                    StatusCodes.Status400BadRequest,
                    BuildProblem(context, StatusCodes.Status400BadRequest, "Bad Request", bre.Message, traceId)
                ),

                ForbiddenException fe => (
                    StatusCodes.Status403Forbidden,
                    BuildProblem(context, StatusCodes.Status403Forbidden, "Forbidden", fe.Message, traceId)
                ),

                // (Optional nhưng nên có)
                NotFoundException nfe => (
                    StatusCodes.Status404NotFound,
                    BuildProblem(context, StatusCodes.Status404NotFound, "Not Found", nfe.Message, traceId)
                ),

                // fallback cho các lỗi “nghiệp vụ” hay gặp
                InvalidOperationException ioe => (
                    StatusCodes.Status400BadRequest,
                    BuildProblem(context, StatusCodes.Status400BadRequest, "Bad Request", ioe.Message, traceId)
                ),

                ArgumentException ae => (
                    StatusCodes.Status400BadRequest,
                    BuildProblem(context, StatusCodes.Status400BadRequest, "Bad Request", ae.Message, traceId)
                ),

                UnauthorizedAccessException uae => (
                    StatusCodes.Status403Forbidden,
                    BuildProblem(context, StatusCodes.Status403Forbidden, "Forbidden", uae.Message, traceId)
                ),

                _ => (
                    StatusCodes.Status500InternalServerError,
                    BuildProblem(context, StatusCodes.Status500InternalServerError, "Internal Server Error",
                        "An unexpected error occurred.", traceId)
                )
            };

            if (status >= 500)
                _logger.LogError(ex, "Unhandled exception. TraceId={TraceId}", traceId);
            else
                _logger.LogWarning(ex, "Handled exception. TraceId={TraceId}", traceId);

            context.Response.StatusCode = status;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsJsonAsync(problem);
        }
    }

    private static ProblemDetails BuildProblem(HttpContext ctx, int status, string title, string detail, string traceId)
    {
        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Instance = ctx.Request.Path
        };
        problem.Extensions["traceId"] = traceId;
        return problem;
    }

    private static ValidationProblemDetails BuildValidationProblem(HttpContext ctx, FluentValidation.ValidationException ex, string traceId)
    {
        var errors = ex.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(x => x.ErrorMessage).ToArray());

        var problem = new ValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Validation Failed",
            Detail = "One or more validation errors occurred.",
            Instance = ctx.Request.Path
        };
        problem.Extensions["traceId"] = traceId;
        return problem;
    }
}
