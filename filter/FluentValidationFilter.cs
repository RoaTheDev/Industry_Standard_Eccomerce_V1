using Ecommerce_site.Dto;
using Ecommerce_site.Util;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Ecommerce_site.filter;

public class FluentValidationFilter : ActionFilterAttribute
{
    /// <summary>
    /// The name of the validation method to invoke on the validator.
    /// This must match the method name in the <see cref="IValidator{T}"/> interface.
    /// </summary>
    private const string ValidationMethodName = "ValidateAsync";

    public override async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        // Determine if the current request is a read operation (GET, HEAD, OPTIONS)
        var httpMethod = context.HttpContext.Request.Method;
        if (IsReadOperation(httpMethod))
        {
            // Skip validation for read operations
            await next();
            return;
        }

        // Get the list of action parameters
        var actionParameters = context.ActionDescriptor.Parameters;
        foreach (var parameter in actionParameters)
        {
            // Check if the parameter is bound from the request body
            if (parameter.BindingInfo?.BindingSource == BindingSource.Body)
            {
                // Get the argument value for this parameter
                var actionArgument = context.ActionArguments[parameter.Name];
                if (actionArgument == null) continue; // Skip if null

                // Validate the argument
                if (!await ValidateArgumentAsync(actionArgument, context))
                {
                    // If validation fails, the result is already set in context
                    return;
                }
            }
        }

        // If no validation errors, proceed to the next action
        await next();
    }

    private bool IsReadOperation(string httpMethod)
    {
        return httpMethod == HttpMethods.Get || httpMethod == HttpMethods.Head || httpMethod == HttpMethods.Options ||
               httpMethod == HttpMethods.Delete ;
    }

    private async Task<bool> ValidateArgumentAsync(object actionArgument, ActionExecutingContext context)
    {
        // Get the type of the argument
        var argumentType = actionArgument.GetType();
        // Get the validator type for this argument type
        var validatorType = typeof(IValidator<>).MakeGenericType(argumentType);
        // Retrieve the validator from the request services
        var validator = context.HttpContext.RequestServices.GetService(validatorType);
        if (validator == null) return true; // No validator found, consider valid

        // Find the ValidateAsync method on the validator
        var validateMethod =
            validatorType.GetMethod(ValidationMethodName, [argumentType, typeof(CancellationToken)]);
        if (validateMethod == null) return true; // No ValidateAsync method found

        // Invoke the ValidateAsync method
        var validationResult = await (Task<FluentValidation.Results.ValidationResult>)validateMethod.Invoke(
            validator, [actionArgument, CancellationToken.None])!;

        // Check if validation was successful
        if (!validationResult.IsValid)
        {
            // Create a list of validation errors
            var errorList = validationResult.Errors
                .Select(e => new ValidationErrorDetails
                {
                    Field = e.PropertyName,
                    Reason = e.ErrorMessage
                })
                .ToList();

            // Create ProblemDetails with validation errors
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

            // Set the result to a Bad Request with ProblemDetails
            context.Result = new BadRequestObjectResult(problemDetails);
            return false;
        }

        return true;
    }
}