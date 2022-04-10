using System.Collections.Generic;
using System.Threading.Tasks;
using Suggestions.Common.Options;
using Suggestions.Infrastructure.Services.Abcp.Model;

namespace Suggestions.Logic.Services.Abcp
{
    public interface IAbcpService
    {
        /// <summary>
        /// Параметры ABCP
        /// </summary>
        AbcpOptions Options { get; }

        /// <summary>
        /// Возвращает подсказки по номеру детали
        /// </summary>
        /// <param name="searchString">Поисковая строка</param>
        Task<IList<AbcpSearchSuggestion>> GetSearchSuggestions(string searchString);

        /// <summary>
        /// Возвращает сопоставление производителей ABCP к производителям Emex
        /// </summary>
        Task<IDictionary<string, string>> GetAbcpToEmexBrandMapping();
    }
}