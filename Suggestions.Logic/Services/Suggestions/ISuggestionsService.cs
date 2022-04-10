using System.Collections.Generic;
using System.Threading.Tasks;
using Suggestions.Logic.Services.Suggestions.Model;

namespace Suggestions.Logic.Services.Suggestions
{
    public interface ISuggestionsService
    {
        Task<IList<SearchSuggestion>> GetSearchSuggestions(string searchString);
    }
}