using System.ComponentModel.DataAnnotations;
using Suggestions.Common.Validation;

namespace Suggestions.Common.Options
{
    /// <summary>
    /// Настройки сервиса истории поиска
    /// </summary>
    public class SearchHistoryOptions
    {
        /// <summary>
        /// Адрес
        /// </summary>
        [NotEmptyString]
        public string Url { get; set; }

        /// <summary>
        /// Таймаут в мс.
        /// </summary>
        [GreaterThanZero]
        public int TimeoutMs { get; set; }

        /// <summary>
        /// Включена ли история поиска для гостей
        /// </summary>
        public bool EnabledForGuests { get; set; }

        /// <summary>
        /// Включена ли история поиска для пользователей
        /// </summary>
        public bool EnabledForUsers { get; set; }
    }
}
