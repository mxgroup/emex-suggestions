using MediatR;
using Suggestions.Logic.UseCases.Model;

namespace Suggestions.Logic.UseCases.GetSearchSuggestionsWithUserSearchHistory
{
    public class
        GetSearchSuggestionsWithUserSearchHistoryRequest : IRequest<GetSearchSuggestionsWithSearchHistoryResponse>
    {
        public long UserId { get; set; }

        public string SearchString { get; set; }
    }
}