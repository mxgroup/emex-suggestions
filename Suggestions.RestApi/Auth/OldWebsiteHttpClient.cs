using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Suggestions.RestApi.Auth
{
    public class OldWebsiteHttpClient
    {
        private readonly HttpClient _httpClient;
        private readonly HttpContext _httpContext;

        public OldWebsiteHttpClient(HttpClient httpClient, IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpContext = httpContextAccessor.HttpContext;

            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(configuration.GetValue<string>("AuthBaseAddress"));
        }

        public Task<(JsonResponse Result, HttpResponseMessage HttpResponse, string ParsingResponseErrorMessage)>
            PrepareFormAndPostAsync<TRequest>(
                string executeUrl,
                TRequest requestModel)
        {
            var parametersDictionary = ConvertToDictionary(requestModel);

            return PrepareFormAndPostAsync(executeUrl, parametersDictionary);
        }

        public Task<(JsonResponse Result, HttpResponseMessage HttpResponse, string ParsingResponseErrorMessage)>
            PrepareFormAndPostAsync(
                string executeUrl,
                Dictionary<string, string> requestParameters = null)
        {
            var parametersDictionary = requestParameters ?? new Dictionary<string, string>();
            var content = new FormUrlEncodedContent(parametersDictionary);
            content.Headers.ContentEncoding.Add(Encoding.UTF8.WebName);

            return PrepareAndPostAsync<JsonResponse>(executeUrl, content);
        }

        private async Task<(TResponse Result, HttpResponseMessage HttpResponse, string ParsingResponseErrorMessage)>
            PrepareAndPostAsync<TResponse>(
                string executeUrl,
                HttpContent content)
            where TResponse : class
        {
            string parsingResponseErrorMessage = null;
            TResponse result = null;

            var request = CreateRequest(HttpMethod.Post, executeUrl, content);
            SetHeaders(request);

            var httpResponse = await _httpClient.SendAsync(request);
            var resultStr = await httpResponse.Content.ReadAsStringAsync();

            try
            {
                result = resultStr.FromJson<TResponse>();
            }
            catch (Exception e)
            {
                parsingResponseErrorMessage = e.Message;
            }

            return (result, httpResponse, parsingResponseErrorMessage);
        }

        private static HttpRequestMessage CreateRequest(HttpMethod httpMethod, string url, HttpContent content)
        {
            return new HttpRequestMessage(httpMethod, url)
            {
                Content = content
            };
        }

        private static Dictionary<string, string> ConvertToDictionary<T>(T model)
        {
            return model.GetType()
                .GetProperties()
                .ToDictionary(property => property.Name, property => property.GetValue(model)?.ToString());
        }

        private void SetHeaders(HttpRequestMessage request)
        {
            request.Headers.Add("X-Requested-With", "XMLHttpRequest");

            var host = _httpContext.Request.GetTypedHeaders()?.Host.Value;
            if (!string.IsNullOrEmpty(host))
            {
                request.Headers.Add("Host", host);
            }

            var authCookieString = GetAuthCookieString(_httpContext.Request);
            if (!string.IsNullOrEmpty(authCookieString))
            {
                request.Headers.Add("Cookie", authCookieString);
            }
        }

        public static string GetAuthCookieString(HttpRequest request)
        {
            var authCookies = request.Cookies
                .Where(x => x.Key.Contains(Constants.Cookies.Authorization) ||
                            x.Key.Contains(Constants.Cookies.VisitorId)).Select(x => $"{x.Key}={x.Value}");
            return string.Join(";", authCookies);
        }
    }

    public class JsonResponse : JsonResponse<object>
    {
    }

    public class JsonResponse<TData>
    {
        public TData Data;

        public JsonResponseErrorCode ErrorCode;

        public string Message;
        public JsonResponseState State;
    }

    public enum JsonResponseState
    {
        Ok = 0,

        Error = -1
    }

    public enum JsonResponseErrorCode
    {
        Business = 0,

        System = 1,

        InvalidContext = 2
    }
}