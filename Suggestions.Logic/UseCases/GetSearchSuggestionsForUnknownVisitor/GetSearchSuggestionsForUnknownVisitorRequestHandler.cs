using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Suggestions.Logic.Services.Suggestions;
using Suggestions.Logic.UseCases.Model;

namespace Suggestions.Logic.UseCases.GetSearchSuggestionsForUnknownVisitor
{
    /// <summary>
    /// Возвращает поисковые подсказки для неопределенного пользователя и посетителя
    /// </summary>
    public class GetSearchSuggestionsForUnknownVisitorRequestHandler : IRequestHandler<
        GetSearchSuggestionsForUnknownVisitorRequest, GetSearchSuggestionsWithSearchHistoryResponse>
    {
        private readonly ISuggestionsService _suggestionsService;

        public GetSearchSuggestionsForUnknownVisitorRequestHandler(ISuggestionsService suggestionsService)
        {
            _suggestionsService = suggestionsService;
        }

        public async Task<GetSearchSuggestionsWithSearchHistoryResponse> Handle(
            GetSearchSuggestionsForUnknownVisitorRequest request, CancellationToken cancellationToken)
        {
            var suggestions = await _suggestionsService.GetSearchSuggestions(request.SearchString);

            return new GetSearchSuggestionsWithSearchHistoryResponse
            {
                Suggestions = suggestions
            };
        }
    }
}