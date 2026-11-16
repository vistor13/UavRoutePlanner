using ErrorOr;
using UavRoutePlanner.BLL.Models.Auth;
using UavRoutePlanner.BLL.Models.Dto.Route;
using UavRoutePlanner.BLL.Models.Dto.UavProfile;

namespace UavRoutePlanner.Api.Extensions;

public static class ErrorOrExtensions
{
    public static IResult ToResult<T>(this ErrorOr<T> result) =>
        result.Match(Results.Ok, ToProblem);

    public static IResult ToResult(this ErrorOr<Created> result) =>
        result.Match(_ => Results.StatusCode(StatusCodes.Status201Created), ToProblem);

    public static IResult ToResult(this ErrorOr<Updated> result) =>
        result.Match(_ => Results.NoContent(), ToProblem);

    public static IResult ToFileResult(this ErrorOr<ExportedFileDto> result) =>
        result.Match(file => Results.File(file.Content, file.ContentType, file.FileName), ToProblem);

    public static IResult ToPhotoResult(this ErrorOr<UavProfilePhotoDto> result) =>
        result.Match(photo => Results.File(photo.Data, photo.ContentType), ToProblem);

    public static IResult ToResult(this ErrorOr<Deleted> result) =>
        result.Match(_ => Results.NoContent(), ToProblem);

    private static IResult ToProblem(List<Error> errors)
    {
        var firstError = errors[0];

        var statusCode = firstError.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Failure => StatusCodes.Status502BadGateway,
            _ when firstError.NumericType == AuthErrors.TooManyAttemptsType => StatusCodes.Status429TooManyRequests,
            _ => StatusCodes.Status500InternalServerError
        };

        return Results.Problem(statusCode: statusCode, title: firstError.Description);
    }
}
