using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Suggestions.Common.Extensions
{
    public static class ServiceCollectionExtension
    {
        /// <summary>
        /// Возвращает настройки, при этом, если они регистрировались с .ValidateDataAnnotations(), то они будут проверены
        /// </summary>
        public static TOptions GetValidatedOptions<TOptions>(this IServiceCollection services) where TOptions : class
        {
            return services.BuildServiceProvider().GetRequiredService<IOptions<TOptions>>().Value;
        }
    }
}
