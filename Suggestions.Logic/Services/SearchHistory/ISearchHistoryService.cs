using System;
using System.Threading.Tasks;
using Suggestions.Infrastructure.Services.SearchHistory.Model;

namespace Suggestions.Logic.Services.SearchHistory
{
    /// <summary>
    /// Сервис истории поиска
    /// </summary>
    public interface ISearchHistoryService
    {
        /// <summary>
        /// Возвращает историю поиска пользователя, фильтруя по строке поиска
        /// </summary>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <param name="searchString">Строка поиска</param>
        Task<GetSearchHistoryResponse> GetUserSearchHistoryFiltered(long userId, string searchString);

        /// <summary>
        /// Возвращает историю поиска гостя, фильтруя по строке поиска
        /// </summary>
        /// <param name="guestId">Идентификатор гостя</param>
        /// <param name="searchString">Строка поиска</param>
        Task<GetSearchHistoryResponse> GetGuestSearchHistoryFiltered(Guid guestId, string searchString);
    }
}