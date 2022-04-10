namespace Suggestions.Infrastructure.Services.Abcp.Model
{
    /// <summary>
    /// Модель возвращаемая методом /search/tips
    /// </summary>
    public class AbcpSearchSuggestion
    {
        public string Brand { get; set; }

        public string Number { get; set; }

        public string Description { get; set; }
    }
}