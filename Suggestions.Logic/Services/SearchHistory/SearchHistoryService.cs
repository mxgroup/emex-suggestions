using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Suggestions.Common;
using Suggestions.Common.Helpers;
using Suggestions.Common.Options;
using Suggestions.Infrastructure.Services.SearchHistory;
using Suggestions.Infrastructure.Services.SearchHistory.Model;

namespace Suggestions.Logic.Services.SearchHistory
{
    public class SearchHistoryService : ISearchHistoryService
    {
        private readonly ISearchHistoryApi _searchHistoryApi;
        private readonly IAsyncPolicy<GetSearchHistoryResponse> _searchHistoryPolicy;
        private readonly SearchHistoryOptions _options;

        public SearchHistoryService(ILogger<SearchHistoryService> logger, ISearchHistoryApi searchHistoryApi, IOptionsSnapshot<SearchHistoryOptions> searchHistoryOptions)
        {
            _searchHistoryPolicy = PollyHelper.WithLoggingTimeoutAndFallback(logger, null, TimeSpan.FromMilliseconds(searchHistoryOptions.Value.TimeoutMs), new GetSearchHistoryResponse());
            _searchHistoryApi = searchHistoryApi;
            _options = searchHistoryOptions.Value;
        }

        public async Task<GetSearchHistoryResponse> GetUserSearchHistoryFiltered(long userId, string searchString)
        {
            if (!_options.EnabledForUsers)
            {
                return new GetSearchHistoryResponse();
            }

            var resp = await _searchHistoryPolicy.ExecuteAsync(
                async c => await _searchHistoryApi.GetUserSearchHistory(userId),
                new Context("SearchHistoryApi.GetUserSearchHistory").WithArgs(new { userId }));
            FilterSearchHistory(resp, searchString);
            return resp;
        }

        public async Task<GetSearchHistoryResponse> GetGuestSearchHistoryFiltered(Guid guestId, string searchString)
        {
            if (!_options.EnabledForGuests)
            {
                return new GetSearchHistoryResponse();
            }

            var resp = await _searchHistoryPolicy.ExecuteAsync(
                async c => await _searchHistoryApi.GetGuestSearchHistory(guestId),
                new Context("SearchHistoryApi.GetGuestSearchHistory").WithArgs(new { guestId }));
            FilterSearchHistory(resp, searchString);
            return resp;
        }

        private void FilterSearchHistory(GetSearchHistoryResponse sh, string searchString)
        {
            sh.Items = sh.Items.Where(d =>
            {
                if (d.Kind == SearchHistoryItemKind.Detail)
                {
                    return CheckIfContainsSubstring(d.DetailNum, searchString) ||
                           CheckIfContainsSubstring(d.DetailName, searchString);
                }

                return CheckIfContainsSubstring(d.Vin, searchString) ||
                       CheckIfContainsSubstring(d.VinDescription, searchString);
            }).ToList();
        }

        private bool CheckIfContainsSubstring(string str, string substring)
        {
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }

            if (string.IsNullOrEmpty(substring))
            {
                return true;
            }

            return str.ToUpperInvariant().Contains(substring.ToUpperInvariant());
        }
    }
}