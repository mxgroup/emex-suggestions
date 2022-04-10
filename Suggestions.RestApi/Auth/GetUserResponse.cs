namespace Suggestions.RestApi.Auth
{
    /// <summary>
    /// Модель ответа account.mvc/getuser
    /// </summary>
    public class GetUserResponse
    {
        public long UserId { get; set; }

        public string Name { get; set; }

        public string Surname { get; set; }

        public string Phone { get; set; }

        public string Email { get; set; }

        public string VisitorType { get; set; }

        public long? LocationId { get; set; }

        public string Version { get; set; }

        public bool? MultiBasketAllowed { get; set; }

        /// <summary>
        /// Возвращает 2, если пользователь создан на новом сайте при оформлении заказа
        /// В других случаях возвращает 1 или null
        /// </summary>
        public int? SiteVersionAtRegistration { get; set; }

        /// <summary>
        /// Был ли хоть один заказ у покупателя
        /// </summary>
        public bool? WasPurchase { get; set; }

        public string OptovikLogo { get; set; }
    }
}