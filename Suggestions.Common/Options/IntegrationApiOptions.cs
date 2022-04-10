using System.ComponentModel.DataAnnotations;
using Suggestions.Common.Validation;

namespace Suggestions.Common.Options
{
    /// <summary>
    /// Настройки Integration.Api
    /// </summary>
    public class IntegrationApiOptions
    {
        /// <summary>
        /// Адрес
        /// </summary>
        [NotEmptyString]
        public string Url { get; set; }

        /// <summary>
        /// Токен
        /// </summary>
        [NotEmptyString]
        public string Token { get; set; }
    }
}