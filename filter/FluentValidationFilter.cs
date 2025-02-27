using Ecommerce_site.Dto;
using Ecommerce_site.Util;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Ecommerce_site.filter;

public class FluentValidationFilter : ActionFilterAttribute
{
    // this is the method name for validation. It must be this since it's from fluent validation "ValidateAsync"
    private const string ValidationMethodName = "ValidateAsync";

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        //     context.ActionArguments contains all parameters passed to the controller action
        //   For example, if your action takes AddressCreationRequest request, this will be one of the arguments
        foreach (var argument in context.ActionArguments)
        {
            var argumentValue = argument.Value;

            if (argumentValue == null) continue;

            var argumentType = argumentValue.GetType();
            var validatorType = typeof(IValidator<>).MakeGenericType(argumentType);

            if (context.HttpContext.RequestServices.GetService(validatorType) is not IValidator validator) continue;

            var validateMethod = validator.GetType()
                .GetMethod(ValidationMethodName, [argumentType, typeof(CancellationToken)]);

            if (validateMethod == null) continue;

            var validationResult = await ((Task<FluentValidation.Results.ValidationResult>)validateMethod.Invoke(
                validator, [argumentValue, CancellationToken.None])!);

            if (!validationResult.IsValid)
            {
                var errorList = validationResult.Errors
                    .Select(e => new ValidationErrorDetails
                    {
                        Field = e.PropertyName,
                        Reason = e.ErrorMessage
                    })
                    .ToList();

                var problemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = GetStatusTitle.GetTitleForStatus(StatusCodes.Status400BadRequest),
                    Detail = "Validation errors.",
                    Extensions =
                    {
                        ["errors"] = errorList
                    }
                };

                context.Result = new BadRequestObjectResult(problemDetails);
                return;
            }
        }

        await next();
    }
}