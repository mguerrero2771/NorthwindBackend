using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NorthWind.Exceptions.Entities.Resources;
using Microsoft.AspNetCore.Diagnostics;
using NorthWind.Exceptions.Entities.Extensions;

namespace NorthWind.Exceptions.Entities.ExceptionHandlers;

internal class UnhandledExceptionHandler(ILogger<UnhandledExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        ProblemDetails Details = new ProblemDetails();
        Details.Status = StatusCodes.Status500InternalServerError;
        Details.Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1";
        Details.Title = ExceptionMessages.UnhandledExceptionTitle;
        // Incluir detalles explícitos del error para facilitar el diagnóstico
        Details.Detail = string.Join(" | ", new[]
        {
            ExceptionMessages.UnhandledExceptionDetail,
            $"Message: {exception.Message}",
            exception.InnerException != null ? $"Inner: {exception.InnerException.Message}" : null
        }.Where(s => !string.IsNullOrWhiteSpace(s)));
        Details.Instance = $"{nameof(ProblemDetails)}/{exception.GetType()}";
        Details.Extensions["traceId"] = httpContext.TraceIdentifier;

        logger.LogError(exception, ExceptionMessages.UnhandledExceptionTitle);

        await httpContext.WriteProblemDetailsAsync(Details);

        return true;

    }
}
