using MediatR;
using Suggestions.Logic.UseCases.Model;

namespace Suggestions.Logic.UseCases.GetSearchSuggestionsForUnknownVisitor
{
    public class GetSearchSuggestionsForUnknownVisitorRequest : IRequest<GetSearchSuggestionsWithSearchHistoryResponse>
    {
        public string SearchString { get; set; }
    }
}