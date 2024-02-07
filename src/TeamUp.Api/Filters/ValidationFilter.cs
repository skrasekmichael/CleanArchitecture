using FluentValidation;
using FluentValidation.Results;

using TeamUp.Contracts;

namespace TeamUp.Api.Filters;

public class ValidationFilter : IEndpointFilter
{
	private readonly IServiceProvider _services;
	private readonly ILogger<ValidationFilter> _logger;

	public ValidationFilter(IServiceProvider services, ILogger<ValidationFilter> logger)
	{
		_services = services;
		_logger = logger;
	}

	public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
	{
		var paramToValidate = context.Arguments.FirstOrDefault(param => param is IRequestBody);
		if (paramToValidate is not null)
		{
			var validatorType = typeof(IValidator<>).MakeGenericType(paramToValidate.GetType());
			var validator = _services.GetService(validatorType);

			if (validator is not null)
			{
				var methodInfo = validatorType.GetMethod(nameof(IValidator.Validate));

				var validationResult = (ValidationResult)methodInfo!.Invoke(validator, [paramToValidate])!;
				if (!validationResult.IsValid)
				{
					return Results.ValidationProblem(
						errors: validationResult.ToDictionary(),
						title: "ValidationErrors",
						detail: "One or more validation errors occurred.",
						statusCode: StatusCodes.Status400BadRequest);
				}
			}
			else
			{
				_logger.LogCritical("Endpoint {endpoint} accepts request of type {requestType} which does not have validator.", context.HttpContext.Request.Path, paramToValidate.GetType());
			}
		}

		return await next.Invoke(context);
	}
}
