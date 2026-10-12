using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Partners.Api.Errors;

public static class InvalidModelStateResponse
{
    public static IActionResult Create(ActionContext context)
    {
        var modelState = new ModelStateDictionary();
        foreach (var (key, entry) in context.ModelState)
        {
            foreach (var error in entry.Errors)
            {
                modelState.AddModelError(ToField(key), ToMessage(key, error));
            }
        }

        var factory = context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
        var problem = factory.CreateValidationProblemDetails(context.HttpContext, modelState,
            StatusCodes.Status400BadRequest);

        return new BadRequestObjectResult(problem)
        {
            ContentTypes = { "application/problem+json" }
        };
    }

    private static string ToField(string key)
    {
        var field = key.TrimStart('$', '.');
        return field.Length == 0 ? "body" : ErrorKeys.ToCamelCase(field);
    }

    private static string ToMessage(string key, ModelError error)
    {
        if (!string.IsNullOrEmpty(error.ErrorMessage))
        {
            return error.ErrorMessage;
        }

        return key.TrimStart('$', '.').Length == 0
            ? "The request body is not valid JSON."
            : "The value is not valid for this field.";
    }
}
