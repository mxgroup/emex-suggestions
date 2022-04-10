using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Suggestions.RestApi.Auth
{
    /// <summary>
    /// Получает из кук VisitorId
    /// </summary>
    public class VisitorTrackingMiddleware
    {
        private readonly RequestDelegate _next;

        public VisitorTrackingMiddleware(RequestDelegate next)
        {
            this._next = next;
        }

        public async Task InvokeAsync(HttpContext context, IConfiguration config)
        {
            var visitorId = context.Request.Cookies[Constants.Cookies.VisitorId];
            if (Guid.TryParse(visitorId, out var visitorIdGuid))
            {
                context.Request.HttpContext.Items[Constants.Cookies.VisitorId] = visitorIdGuid;
            }

            await _next.Invoke(context);
        }
    }
}
