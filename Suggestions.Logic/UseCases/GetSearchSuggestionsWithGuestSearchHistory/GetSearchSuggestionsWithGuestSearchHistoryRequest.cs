using System;
using MediatR;
using Suggestions.Logic.UseCases.Model;

namespace Suggestions.Logic.UseCases.GetSearchSuggestionsWithGuestSearchHistory
{
    public class
        GetSearchSuggestionsWithGuestSearchHistoryRequest : IRequest<GetSearchSuggestionsWithSearchHistoryResponse>
    {
        public Guid GuestId { get; set; }

        public string SearchString { get; set; }
    }
}