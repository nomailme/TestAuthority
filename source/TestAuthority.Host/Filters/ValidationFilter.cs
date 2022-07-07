using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TestAuthority.Host.Contracts;

namespace TestAuthority.Host.Filters;

/// <summary>
/// Validation filter.
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global
public class ValidationFilter : IAsyncActionFilter
{
    /// <inheritdoc />
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.ModelState.IsValid)
        {
            var errorsInModelState = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.Errors.Select(x => x.ErrorMessage)).ToArray();

            var errorResponse = new ErrorResponse();

            foreach (var (key, value) in errorsInModelState)
            {
                foreach (var subError in value)
                {
                    var errorModel = new ErrorModel
                    {
                        FieldName = key,
                        Message = subError
                    };

                    errorResponse.Errors.Add(errorModel);
                }
            }
            context.Result = new BadRequestObjectResult(errorResponse);
            return;
        }

        await next();
    }
}
