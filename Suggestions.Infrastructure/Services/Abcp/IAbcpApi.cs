using System.Collections.Generic;
using System.Threading.Tasks;
using Refit;
using Suggestions.Infrastructure.Services.Abcp.Model;

namespace Suggestions.Infrastructure.Services.Abcp
{
    /// <summary>
    /// API сайта abcp.ru https://www.abcp.ru/wiki/API.ABCP.Client
    /// </summary>
    public interface IAbcpApi
    {
        /// <summary>
        /// Возвращает подсказки по номеру детали
        /// </summary>
        /// <param name="login">Логин</param>
        /// <param name="password">Пароль</param>
        /// <param name="detailNum">Номер детали</param>
        /// <returns></returns>
        [Get("/search/tips?userlogin={login}&userpsw={password}&number={detailNum}")]
        Task<IList<AbcpSearchSuggestion>> GetSuggestionsAsync(string login, string password, string detailNum);
    }
}