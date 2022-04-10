using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Suggestions.Logic.Services.Abcp;
using Suggestions.Logic.Services.SearchHistory;
using Suggestions.Logic.Services.Suggestions;

namespace Suggestions.Logic
{
    public static class LogicModule
    {
        public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<ISuggestionsService, SuggestionsService>();
            services.AddScoped<IAbcpService, AbcpService>();
            services.AddScoped<ISearchHistoryService, SearchHistoryService>();
        }
    }
}