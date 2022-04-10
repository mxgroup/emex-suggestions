using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Suggestions.Common.Exceptions;

namespace Suggestions.RestApi.Extensions.Filters
{
    public class BadOperationExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            var statusCodeException = context.Exception as BadOperationException;
            if (statusCodeException == null)
            {
                return;
            }

            context.Result =
                new BadRequestObjectResult(statusCodeException.Message);
            context.ExceptionHandled = true;
        }
    }
}