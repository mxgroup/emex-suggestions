using Suggestions.Common.Validation;

namespace Suggestions.Common.Options
{
    /// <summary>
    /// Настройки ABCP
    /// </summary>
    public class AbcpOptions
    {
        /// <summary>
        /// Адрес API
        /// </summary>
        [NotEmptyString]
        public string Url { get; set; }

        /// <summary>
        /// Логин
        /// </summary>
        [NotEmptyString]
        public string Login { get; set; }

        /// <summary>
        /// Пароль
        /// </summary>
        [NotEmptyString]
        public string Password { get; set; }

        /// <summary>
        /// Таймаут получения подсказок (мс)
        /// </summary>
        [GreaterThanZero]
        public int SuggestionsTimeoutMs { get; set; }

        /// <summary>
        /// Запрашивать ли подсказки 
        /// </summary>
        public bool SuggestionsEnabled { get; set; }
    }
}