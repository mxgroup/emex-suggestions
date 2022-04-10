using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Suggestions.Common.Extensions;

namespace Suggestions.RestApi.Auth
{
    public class AuthLogic : IAuthLogic
    {
        private readonly IMemoryCache _cache;
        private readonly string _emexBaseAddress;
        private readonly HttpClientWrapper _httpClientWrapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string CacheKey = "AuthByCookie";

        public AuthLogic(
            IConfiguration configuration,
            IMemoryCache cache,
            HttpClientWrapper httpClientWrapper,
            IHttpContextAccessor httpContextAccessor)
        {
            _cache = cache;
            _emexBaseAddress = configuration.GetValue<string>("AuthBaseAddress");
            _httpClientWrapper = httpClientWrapper;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Данные о пользователе
        /// </summary>
        public UserAuth UserAuth { get; private set; }

        /// <summary>
        /// Идентификатор посетителя
        /// </summary>
        public Guid? VisitorId =>
            (Guid?) _httpContextAccessor.HttpContext.Request.HttpContext.Items[Constants.Cookies.VisitorId];

        public async Task<UserAuth> AuthenticateAsync()
        {
            var host = _httpContextAccessor.HttpContext.Request.GetTypedHeaders()?.Host.Value;

            var cookiesString = GetCookieString();
            var cacheKey = GetCacheKey(cookiesString);
            return UserAuth =
                await _cache.Get(cacheKey, async () => await GetUserAsyncInternal(host, cookiesString),
                    TimeSpan.FromMinutes(15));
        }

        private async Task<UserAuth> GetUserAsyncInternal(string host, string cookiesString)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_emexBaseAddress}/account.mvc/getuser");
            request.Headers.Add("Cookie", cookiesString);
            request.Headers.Add("X-Requested-With", "XMLHttpRequest");
            var response = await _httpClientWrapper.Client.SendAsync(request);
            var result = await response.Content.ReadAsStringAsync();
            JsonResponse<GetUserResponse> resultObject;
            try
            {
                resultObject = JsonConvert.DeserializeObject<JsonResponse<GetUserResponse>>(result);
            }
            catch (JsonReaderException)
            {
                return new UserAuth
                {
                    IsSuccess = false,
                    Error = string.Empty
                };
            }

            if (resultObject.State != JsonResponseState.Ok)
            {
                return new UserAuth
                {
                    IsSuccess = false,
                    Error = resultObject.Message
                };
            }

            var userId = Convert.ToInt64(resultObject.Data.UserId);
            var phone = resultObject.Data.Phone ?? string.Empty;
            var locationId = resultObject.Data.LocationId ?? 0;

            var userData = new UserDataWithVersion
            {
                UserId = userId,
                Name = resultObject.Data.Name ?? string.Empty,
                Surname = resultObject.Data.Surname ?? string.Empty,
                Phone = phone,
                FormattedPhone = FormatHelper.FormatPhone(phone),
                Email = resultObject.Data.Email,
                UserType = resultObject.Data.VisitorType,
                Version = resultObject.Data.Version ?? string.Empty,
                LocationId = locationId,
                MultiBasketAllowed = resultObject.Data.MultiBasketAllowed,
                SiteVersionAtRegistration = resultObject.Data.SiteVersionAtRegistration,
                WasPurchase = resultObject.Data.WasPurchase,
                OptovikLogo = resultObject.Data.OptovikLogo
            };

            return new UserAuth
            {
                IsSuccess = userId > 0,
                Data = userData
            };
        }

        private string GetCookieString()
        {
            return OldWebsiteHttpClient.GetAuthCookieString(_httpContextAccessor.HttpContext.Request);
        }

        private string GetCacheKey(string cookiesString)
        {
            return CacheKey + cookiesString;
        }

        public void ClearAuthByCookie()
        {
            var cookiesString = GetCookieString();
            _cache.Remove(GetCacheKey(cookiesString));
        }
    }
}