using System;

namespace Suggestions.Infrastructure.Services.SearchHistory.Model
{
    public class SearchHistoryItem
    {
        public SearchHistoryItemKind Kind { get; set; }

        public DateTime DateTime { get; set; }

        public string DetailNum { get; set; }

        public string DetailName { get; set; }

        public string Vin { get; set; }

        public string VinDescription { get; set; }
    }
}