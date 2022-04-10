using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using Refit;
using Suggestions.Common.Exceptions;
using Suggestions.Common.Extensions;
using Suggestions.Common.Options;
using Suggestions.Infrastructure.Services.Abcp;
using Suggestions.Infrastructure.Services.Integration;
using Suggestions.Infrastructure.Services.SearchHistory;

namespace Suggestions.Infrastructure
{
    public static class InfrastructureModule
    {
        public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // Сервис истории поиска
            services.AddRefitClient<ISearchHistoryApi>(new RefitSettings
                {
                    ContentSerializer = new SystemTextJsonContentSerializer(new JsonSerializerOptions()
                        { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })
                }).ConfigureHttpClient(c =>
                {
                    var searchHistoryOptions = services.GetValidatedOptions<SearchHistoryOptions>();
                    c.BaseAddress = new Uri(searchHistoryOptions.Url);
                })
                .AddPolicyHandler(GetCircuitBreakerPolicy());

            // Сервис ABCP
            services.AddRefitClient<IAbcpApi>(new RefitSettings
                {
                    ContentSerializer = new SystemTextJsonContentSerializer(new JsonSerializerOptions()
                        { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })
                }).ConfigureHttpClient(c =>
                {
                    var abcpOptions = services.GetValidatedOptions<AbcpOptions>();
                    c.BaseAddress = new Uri(abcpOptions.Url);
                })
                .AddPolicyHandler(GetCircuitBreakerPolicy());

            // Сервис Integration.Api
            services.AddRefitClient<IIntegrationApi>(new RefitSettings
            {
                ContentSerializer = new SystemTextJsonContentSerializer(new JsonSerializerOptions()
                    { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })
            }).ConfigureHttpClient(c =>
            {
                var integrationApiOptions = services.GetValidatedOptions<IntegrationApiOptions>();
                c.BaseAddress = new Uri(integrationApiOptions.Url);
                c.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", integrationApiOptions.Token);
            }).AddPolicyHandler(GetCircuitBreakerPolicy());
        }

        private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
        }
    }
}