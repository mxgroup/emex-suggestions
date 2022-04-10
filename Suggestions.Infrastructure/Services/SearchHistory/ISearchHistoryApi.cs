using System;
using System.Threading.Tasks;
using Refit;
using Suggestions.Infrastructure.Services.SearchHistory.Model;

namespace Suggestions.Infrastructure.Services.SearchHistory
{
    /// <summary>
    /// API сервиса истории поиска
    /// </summary>
    public interface ISearchHistoryApi
    {
        /// <summary>
        /// Возвращает историю поиска пользователя
        /// </summary>
        /// <param name="userId">Идентификатор пользователя</param>
        [Get("/users/{userId}")]
        Task<GetSearchHistoryResponse> GetUserSearchHistory(long userId);

        /// <summary>
        /// Возвращает историю поиска гостя
        /// </summary>
        /// <param name="guestId">Идентификатор гостя</param>
        [Get("/guests/{guestId}")]
        Task<GetSearchHistoryResponse> GetGuestSearchHistory(Guid guestId);
    }
}