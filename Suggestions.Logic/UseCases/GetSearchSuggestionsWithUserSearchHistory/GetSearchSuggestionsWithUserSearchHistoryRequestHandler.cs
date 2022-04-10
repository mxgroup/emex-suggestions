using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Suggestions.Infrastructure.Services.SearchHistory.Model;
using Suggestions.Logic.Services.SearchHistory;
using Suggestions.Logic.Services.Suggestions;
using Suggestions.Logic.UseCases.Model;

namespace Suggestions.Logic.UseCases.GetSearchSuggestionsWithUserSearchHistory
{
    /// <summary>
    /// Возвращает поисковые подсказки с историей поиска пользователя
    /// </summary>
    public class GetSearchSuggestionsWithUserSearchHistoryRequestHandler : IRequestHandler<
        GetSearchSuggestionsWithUserSearchHistoryRequest, GetSearchSuggestionsWithSearchHistoryResponse>
    {
        private readonly ISearchHistoryService _searchHistoryService;
        private readonly ISuggestionsService _suggestionsService;

        public GetSearchSuggestionsWithUserSearchHistoryRequestHandler(ISuggestionsService suggestionsService,
            ISearchHistoryService searchHistoryService)
        {
            _suggestionsService = suggestionsService;
            _searchHistoryService = searchHistoryService;
        }


        public async Task<GetSearchSuggestionsWithSearchHistoryResponse> Handle(
            GetSearchSuggestionsWithUserSearchHistoryRequest request, CancellationToken cancellationToken)
        {
            var suggestionsTask = _suggestionsService.GetSearchSuggestions(request.SearchString);
            var searchHistoryTask = _searchHistoryService.GetUserSearchHistoryFiltered(request.UserId, request.SearchString);
            await Task.WhenAll(suggestionsTask, searchHistoryTask);

            return new GetSearchSuggestionsWithSearchHistoryResponse
            {
                Suggestions = suggestionsTask.Result,
                DetailSearchHistory = searchHistoryTask.Result.Items?.Where(i => i.Kind == SearchHistoryItemKind.Detail)
                    .OrderByDescending(i => i.DateTime)
                    .Select(i => new DetailSearchHistoryItem { DetailNum = i.DetailNum, Name = i.DetailName }).ToList(),
                VinSearchHistory = searchHistoryTask.Result.Items?.Where(i => i.Kind == SearchHistoryItemKind.Vin)
                    .OrderByDescending(i => i.DateTime)
                    .Select(i => new VinSearchHistoryItem { Vin = i.Vin, Description = i.VinDescription }).ToList()
            };
        }
    }
}