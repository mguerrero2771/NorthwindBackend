using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NorthWind.Exceptions.Entities.Exceptions;
using NorthWind.Exceptions.Entities.Extensions;
using NorthWind.Exceptions.Entities.Resources;

namespace NorthWind.Exceptions.Entities.ExceptionHandlers;

internal class UpdateExceptionHandler(ILogger<UpdateExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        bool Handled = false;

        if (exception is UpdateException Ex)
        {

            ProblemDetails Details = new ProblemDetails();
            Details.Status = StatusCodes.Status500InternalServerError;
            Details.Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1";
            Details.Title = ExceptionMessages.UpdateExceptionTitle;
            Details.Detail = string.Join(" | ", new[]
            {
                ExceptionMessages.UpdateExceptionDetail,
                !string.IsNullOrWhiteSpace(Ex.Message) ? $"Message: {Ex.Message}" : null,
                (Ex.InnerException != null && !string.IsNullOrWhiteSpace(Ex.InnerException.Message)) ? $"Inner: {Ex.InnerException.Message}" : null,
                (Ex.InnerException?.InnerException != null && !string.IsNullOrWhiteSpace(Ex.InnerException.InnerException.Message)) ? $"InnerInner: {Ex.InnerException.InnerException.Message}" : null
            }.Where(s => !string.IsNullOrWhiteSpace(s)));
            Details.Instance = $"{nameof(ProblemDetails)}/{nameof(UpdateException)}";
            Details.Extensions["traceId"] = httpContext.TraceIdentifier;
            if (Ex.Entities != null) Details.Extensions["entities"] = Ex.Entities;

            logger.LogError(exception, ExceptionMessages.UpdateExceptionTitle + ":" + string.Join(" " + Ex.Entities));


            await httpContext.WriteProblemDetailsAsync(Details);
            Handled = true;
        }

        return Handled;
    }
}
