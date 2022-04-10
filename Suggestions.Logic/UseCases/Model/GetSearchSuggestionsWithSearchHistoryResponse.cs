using System.Collections.Generic;
using Suggestions.Logic.Services.Suggestions.Model;

namespace Suggestions.Logic.UseCases.Model
{
    /// <summary>
    /// Подсказки с историей поиска
    /// </summary>
    public class GetSearchSuggestionsWithSearchHistoryResponse
    {
        public IList<DetailSearchHistoryItem> DetailSearchHistory { get; set; } = new List<DetailSearchHistoryItem>();

        public IList<VinSearchHistoryItem> VinSearchHistory { get; set; } = new List<VinSearchHistoryItem>();

        public IList<SearchSuggestion> Suggestions { get; set; } = new List<SearchSuggestion>();
    }
}