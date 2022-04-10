using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Suggestions.Common;
using Suggestions.Common.Extensions;
using Suggestions.Common.Helpers;
using Suggestions.Common.Options;
using Suggestions.Infrastructure.Services.Abcp;
using Suggestions.Infrastructure.Services.Abcp.Model;
using Suggestions.Infrastructure.Services.Integration;

namespace Suggestions.Logic.Services.Abcp
{
    public class AbcpService : IAbcpService
    {
        private readonly IAbcpApi _abcpApi;
        private readonly IMemoryCache _cache;
        private readonly IIntegrationApi _integrationApi;
        private readonly ILogger<AbcpService> _logger;
        private readonly IAsyncPolicy<IList<AbcpSearchSuggestion>> _suggestionPolicy;

        public AbcpService(IOptionsSnapshot<AbcpOptions> abcpOptions, IAbcpApi abcpApi, IIntegrationApi integrationApi,
            IMemoryCache cache, ILogger<AbcpService> logger)
        {
            Options = abcpOptions.Value;
            _abcpApi = abcpApi;
            _integrationApi = integrationApi;
            _cache = cache;
            _logger = logger;
            _suggestionPolicy = PollyHelper.WithLoggingAndTimeout<IList<AbcpSearchSuggestion>>(_logger, "Abcp.GetSuggestions",
                TimeSpan.FromMilliseconds(Options.SuggestionsTimeoutMs),
                new List<AbcpSearchSuggestion>());
        }

        public AbcpOptions Options { get; }

        public async Task<IList<AbcpSearchSuggestion>> GetSearchSuggestions(string searchString)
        {
            return await _suggestionPolicy.ExecuteAsync(
                async c => await _abcpApi.GetSuggestionsAsync(Options.Login, Options.Password, searchString),
                new Context().WithArgs(new { searchString }));
        }

        public async Task<IDictionary<string, string>> GetAbcpToEmexBrandMapping()
        {
            return await _cache.Get("AbcpToEmexBrandMapping", _integrationApi.GetAbcpToEmexBrandMapping,
                TimeSpan.FromHours(3));
        }
    }
}