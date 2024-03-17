using Microsoft.AspNetCore.Mvc;

using RailwayResult;
using RailwayResult.Errors;

namespace TeamUp.Api.Extensions;

public static class ResultExtensions
{
	public static IResult Match<TOut>(this Result<TOut> result, Func<TOut, IResult> success)
	{
		if (result.IsSuccess)
			return success(result.Value!);

		return result.Error.ToResponse();
	}

	public static IResult Match(this Result result, Func<IResult> success)
	{
		if (result.IsSuccess)
			return success();

		return result.Error.ToResponse();
	}

	public static IResult ToResponse(this Error error)
	{
		return TypedResults.Problem(new ProblemDetails
		{
			Title = error.GetType().Name,
			Detail = error.Message,
			Status = error switch
			{
				AuthenticationError => StatusCodes.Status401Unauthorized,
				AuthorizationError => StatusCodes.Status403Forbidden,
				NotFoundError => StatusCodes.Status404NotFound,
				ValidationError => StatusCodes.Status400BadRequest,
				DomainError => StatusCodes.Status400BadRequest,
				ConflictError => StatusCodes.Status409Conflict,

				InternalError => StatusCodes.Status500InternalServerError,
				_ => StatusCodes.Status500InternalServerError
			}
		});
	}
}
