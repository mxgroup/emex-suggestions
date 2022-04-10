using System.Collections.Generic;
using System.Threading.Tasks;
using Refit;

namespace Suggestions.Infrastructure.Services.Integration
{
    /// <summary>
    /// API сервиса Integration.Api
    /// </summary>
    public interface IIntegrationApi
    {
        /// <summary>
        /// Возвращает сопоставление производителей ABCP к производителям Emex
        /// </summary>
        [Get("/api/abcp/abcp-to-emex-mapping")]
        Task<IDictionary<string, string>> GetAbcpToEmexBrandMapping();
    }
}