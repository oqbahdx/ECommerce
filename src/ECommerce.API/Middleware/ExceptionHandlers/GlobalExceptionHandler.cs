namespace ECommerce.API.Middleware.ExceptionHandlers;

using ECommerce.Application.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IHostEnvironment environment)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(
            exception,
            "An exception occurred while processing request {Method} {Path}",
            httpContext.Request.Method,
            httpContext.Request.Path);

        var problemDetails = CreateProblemDetails(
            httpContext,
            exception,
            environment.IsDevelopment());

        httpContext.Response.StatusCode =
            problemDetails.Status ?? 500;

        await httpContext.Response.WriteAsJsonAsync(
            problemDetails,
            cancellationToken);

        return true;
    }

    private static ProblemDetails CreateProblemDetails(
        HttpContext httpContext,
        Exception exception,
        bool isDevelopment)
    {
        var (statusCode, title) = exception switch
        {
            ValidationException =>
                (StatusCodes.Status400BadRequest, "Validation Failed"),

            NotFoundException =>
                (StatusCodes.Status404NotFound, "Resource Not Found"),

            BadRequestException =>
                (StatusCodes.Status400BadRequest, "Bad Request"),

            ConflictException =>
                (StatusCodes.Status409Conflict, "Conflict"),

            _ =>
                (StatusCodes.Status500InternalServerError,
                    "Internal Server Error")
        };

        var detail = statusCode == StatusCodes.Status500InternalServerError
            ? isDevelopment
                ? exception.Message
                : "An unexpected error occurred."
            : exception.Message;

        var problemDetails = new ProblemDetails
        {
            Type = $"https://httpstatuses.com/{statusCode}",
            Title = title,
            Status = statusCode,
            Detail = detail,
            Instance = httpContext.Request.Path
        };

        if (exception is ValidationException validationException)
        {
            problemDetails.Extensions["errors"] =
                validationException.Errors
                    .GroupBy(error => error.PropertyName)
                    .ToDictionary(
                        group => group.Key,
                        group => group
                            .Select(error => error.ErrorMessage)
                            .ToArray());
        }

        problemDetails.Extensions["traceId"] =
            httpContext.TraceIdentifier;

        return problemDetails;
    }
}