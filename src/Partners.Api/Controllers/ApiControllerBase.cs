using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Partners.Api.Errors;
using Partners.Core.Common;

namespace Partners.Api.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected ActionResult Problem(Error error)
    {
        if (error.Type == ErrorType.Validation)
        {
            var modelState = new ModelStateDictionary();
            foreach (var (field, messages) in error.ValidationErrors ?? new Dictionary<string, string[]>())
            {
                foreach (var message in messages)
                {
                    modelState.AddModelError(ErrorKeys.ToCamelCase(field), message);
                }
            }

            return ValidationProblem(modelState);
        }

        var statusCode = error.Type switch
        {
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

        return Problem(detail: error.Detail, statusCode: statusCode, title: error.Title);
    }
}
