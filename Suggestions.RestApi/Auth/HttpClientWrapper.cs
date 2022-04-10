using System.Net.Http;

namespace Suggestions.RestApi.Auth
{
    /// <summary>
    /// Обёртка для получения HttpClient из IHttpClientFactory через DI.
    /// </summary>
    /// <remarks>
    /// Экземляр HttpClient создаётся как Scoped, но с совместно используемым HttpMessageHandler, управляемым фабрикой.
    /// Автоматическое управление куками отключается при регистрации HttpClientWrapper в DI.
    /// </remarks>
    public class HttpClientWrapper
    {
        /// <summary>
        /// HttpClient, созданный фабрикой
        /// </summary>
        public readonly HttpClient Client;

        public HttpClientWrapper(HttpClient httpClient)
        {
            Client = httpClient;
        }
    }
}