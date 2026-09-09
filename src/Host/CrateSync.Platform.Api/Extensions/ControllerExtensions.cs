using CrateSync.Platform.BuildingBlocks.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace CrateSync.Platform.Api.Extensions;

internal static class ControllerExtensions
{
    public static IActionResult ToProblem(this ControllerBase controller, Error error)
    {
        var statusCode = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError
        };

        return controller.Problem(
            statusCode: statusCode,
            title: error.Code,
            detail: error.Description,
            type: $"https://httpstatuses.com/{statusCode}");
    }
}
