using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NorthWind.Validation.Entities.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NorthWind.Membership.Backend.Core.Extensions;

internal static class ValidationErrorsExtensions
{
    public static ProblemDetails ToProblemDetails(
   this IEnumerable<ValidationError> errors,
   string title, string detail, string instance)
    {
        ProblemDetails Details = new ProblemDetails();
        Details.Status = StatusCodes.Status400BadRequest;
        Details.Type =
        "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1";
        Details.Title = title;
        Details.Detail = detail;
        Details.Instance =
        $"{nameof(ProblemDetails)}/{instance}";
        Details.Extensions.Add("errors", errors);
        return Details;
    }
}
