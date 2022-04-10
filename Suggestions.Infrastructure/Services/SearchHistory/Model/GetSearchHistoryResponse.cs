using System.Collections.Generic;

namespace Suggestions.Infrastructure.Services.SearchHistory.Model
{
    public class GetSearchHistoryResponse
    {
        public IList<SearchHistoryItem> Items { get; set; } = new List<SearchHistoryItem>();
    }
}