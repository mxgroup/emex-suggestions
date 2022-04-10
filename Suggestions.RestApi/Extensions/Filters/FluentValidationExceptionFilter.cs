using System.Linq;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Suggestions.RestApi.Extensions.Filters
{
    public class FluentValidationExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            var statusCodeException = context.Exception as ValidationException;
            if (statusCodeException == null)
            {
                return;
            }

            context.Result =
                new BadRequestObjectResult(
                    statusCodeException.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));
            context.ExceptionHandled = true;
        }
    }
}