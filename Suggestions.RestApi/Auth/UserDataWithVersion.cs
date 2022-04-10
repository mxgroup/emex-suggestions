using Newtonsoft.Json;

namespace Suggestions.RestApi.Auth
{
    /// <summary>
    /// Пользователь сайта и версия сайта, которую пользователю рекомендовано использовать
    /// </summary>
    public class UserDataWithVersion
    {
        /// <summary>
        /// Какую версию сайта показывать
        /// </summary>
        public string Version { get; internal set; }

        /// <summary>
        /// Был ли хоть один заказ у клиента
        /// </summary>
        public bool? WasPurchase { get; internal set; }

        #region IUserData

        /// <summary>
        /// Идентификатор пользователя
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// Имя пользователя
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Фамилия пользователя
        /// </summary>
        public string Surname { get; set; }

        /// <summary>
        /// Телефон пользователя
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// Форматированный телефон пользователя
        /// </summary>
        public string FormattedPhone { get; set; }

        /// <summary>
        /// Главный Email пользователя, хранящийся в таблице Users, очищенный от фиктивных адресов
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Тип пользователя: Guest | Opt | Potr | PotrOld
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string UserType { get; set; }

        /// <summary>
        /// Id офиса, к которому привязан пользователь
        /// </summary>
        public long LocationId { get; set; }

        /// <summary>
        /// Разрешена ли работа с мульти корзиной
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? MultiBasketAllowed { get; set; }

        /// <summary>
        /// Возвращает 2, если пользователь создан на новом сайте при оформлении заказа
        /// В других случаях возвращает 1 или null
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? SiteVersionAtRegistration { get; set; }

        /// <summary>
        /// Вторичный вход и прочее
        /// </summary>
        public string[] Features { get; set; }

        /// <summary>
        /// Лого оптовика
        /// </summary>
        public string OptovikLogo { get; set; }

        #endregion
    }
}