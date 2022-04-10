using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;
using Suggestions.Infrastructure.Services.Abcp;
using Suggestions.Infrastructure.Services.Abcp.Model;
using Suggestions.Infrastructure.Services.Integration;
using Suggestions.Infrastructure.Services.SearchHistory;
using Suggestions.Infrastructure.Services.SearchHistory.Model;
using Suggestions.Logic.Services.Suggestions;
using Suggestions.Logic.Services.Suggestions.Model;
using Suggestions.Logic.UseCases.Model;
using Suggestions.RestApi;
using Suggestions.RestApi.Auth;
using Xunit;

namespace Suggestions.IntegrationTests.Tests
{
    /// <summary>
    /// Тестирует контроллер SuggestionsController
    /// </summary>
    public class SuggestionsControllerTest
    {
        /// <summary>
        /// Проверяет, что метод  GET: search-suggestions возвращает подсказки для любых авторизационных данных,
        /// для пользователей и гостей дополнительно возвращается история поиска, если учетные данные отсутствуют возвращает пустую
        /// историю
        /// </summary>
        [Theory]
        [InlineData("5a7124ca-f571-494f-9544-02734087c3cc", null)]
        [InlineData(null, 435345)]
        [InlineData(null, null)]
        public async Task Returns_Suggestions_Plus_Include_SearchHistory_If_Identity_Is_Present(string guestIdStr,
            long? userId)
        {
            var guestId = guestIdStr != null ? Guid.Parse(guestIdStr) : (Guid?) null;

            var searchHistory = new List<SearchHistoryItem>();
            searchHistory.Add(new SearchHistoryItem
            {
                DateTime = DateTime.Now, DetailName = "Detail1 Name 010", DetailNum = "Detail1 Num",
                Kind = SearchHistoryItemKind.Detail
            });
            searchHistory.Add(new SearchHistoryItem
            {
                DateTime = DateTime.Now, DetailName = "Detail2 Name", DetailNum = "Detail2 Num",
                Kind = SearchHistoryItemKind.Detail
            });
            searchHistory.Add(new SearchHistoryItem
            {
                DateTime = DateTime.Now, Vin = "Vin1 010", VinDescription = "Vin1 desc", Kind = SearchHistoryItemKind.Vin
            });
            searchHistory.Add(new SearchHistoryItem
            {
                DateTime = DateTime.Now, Vin = "Vin2", VinDescription = "Vin2 desc", Kind = SearchHistoryItemKind.Vin
            });

            var expectedVinHistory = searchHistory.Where(s => s.Kind == SearchHistoryItemKind.Vin && s.Vin.Contains("010"))
                .OrderByDescending(s => s.DateTime).Select(s =>
                    new VinSearchHistoryItem { Description = s.VinDescription, Vin = s.Vin });

            var expectedDetailHistory = searchHistory.Where(s => s.Kind == SearchHistoryItemKind.Detail && s.DetailName.Contains("010"))
                .OrderByDescending(s => s.DateTime).Select(s =>
                    new DetailSearchHistoryItem { DetailNum = s.DetailNum, Name = s.DetailName });

            var suggestions = new List<AbcpSearchSuggestion>();
            suggestions.Add(new AbcpSearchSuggestion
                { Brand = "Runway", Number = "010", Description = "Краска черная матовая (265г)" });
            suggestions.Add(new AbcpSearchSuggestion
            {
                Brand = "Nanoprotech", Number = "010", Description = "Смазка подвижных деталей для скутера, 210 мл."
            });
            suggestions.Add(new AbcpSearchSuggestion { Brand = "FEBI", Number = "01089", Description = "Антифриз" });

            // Дополнительно проверяем, что корректно отрабатывает маппинг брендов, убирая один бренд и меняя названия других
            var expectedSuggestions = new List<SearchSuggestion>();
            expectedSuggestions.Add(new SearchSuggestion
                { Brand = "Runway_Emex", Number = "010", Description = "Краска черная матовая (265г)" });
            expectedSuggestions.Add(new SearchSuggestion
                { Brand = "Febi", Number = "01089", Description = "Антифриз" });

            var abcpToEmexMapping = new Dictionary<string, string>();
            abcpToEmexMapping["Runway"] = "Runway_Emex";
            abcpToEmexMapping["FEBI"] = "Febi";

            var integrationApiMock = new Mock<IIntegrationApi>();
            var apiFactory = new WebApplicationFactory<Startup>()
                .WithWebHostBuilder(builder =>
                {
                    builder.UseSetting("IsIntegrationTesting", "true");
                    builder.ConfigureServices(s =>
                    {
                        // IAuthLogic
                        var authLogicMock = new Mock<IAuthLogic>();
                        authLogicMock.Setup(m => m.VisitorId).Returns(guestId);
                        authLogicMock.Setup(m => m.AuthenticateAsync()).Returns(Task.FromResult(new UserAuth
                            { IsSuccess = true, Data = new UserDataWithVersion { UserId = userId ?? 0 } }));
                        s.AddScoped(s => authLogicMock.Object);

                        // ISearchHistoryApi
                        var searchHistoryApiMock = new Mock<ISearchHistoryApi>();
                        if (guestId != null)
                        {
                            searchHistoryApiMock.Setup(m => m.GetGuestSearchHistory(guestId.Value))
                                .Returns(Task.FromResult(new GetSearchHistoryResponse { Items = searchHistory }));
                        }

                        if (userId != null)
                        {
                            searchHistoryApiMock.Setup(m => m.GetUserSearchHistory(userId.Value))
                                .Returns(Task.FromResult(new GetSearchHistoryResponse { Items = searchHistory }));
                        }

                        s.AddScoped(s => searchHistoryApiMock.Object);

                        // IAbcpApi
                        var abcpApiMock = new Mock<IAbcpApi>();
                        abcpApiMock.Setup(m =>
                                m.GetSuggestionsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                            .Returns(Task.FromResult<IList<AbcpSearchSuggestion>>(suggestions));
                        s.AddScoped(s => abcpApiMock.Object);

                        // IIntegrationApi
                        integrationApiMock.Setup(m =>
                                m.GetAbcpToEmexBrandMapping())
                            .Returns(Task.FromResult<IDictionary<string, string>>(abcpToEmexMapping));
                        s.AddScoped(s => integrationApiMock.Object);
                    });
                });

            // Делаем запрос
            var client = apiFactory.CreateClient();
            var response = await client.GetAsync(new Uri(client.BaseAddress, "suggestions/search-suggestions?searchString=010"));

            // Проверяем что ответ 200
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Проверяем данные
            var data = JsonConvert.DeserializeObject<GetSearchSuggestionsWithSearchHistoryResponse>(
                await response.Content.ReadAsStringAsync());

            // Если гость или пользователь опознаны
            if (userId != null || guestId != null)
            {
                data.VinSearchHistory.Should()
                    .BeEquivalentTo(expectedVinHistory, options => options.WithStrictOrdering());
                data.DetailSearchHistory.Should()
                    .BeEquivalentTo(expectedDetailHistory, options => options.WithStrictOrdering());
            }
            else
            {
                data.VinSearchHistory.Should().BeEmpty();
                data.DetailSearchHistory.Should().BeEmpty();
            }

            data.Suggestions.Should().BeEquivalentTo(expectedSuggestions);

            // Проверяем что работает кэш, делаем второй вызов
            await client.GetAsync(new Uri(client.BaseAddress, "suggestions/search-suggestions?searchString=010"));
            // Метод не должен был вызываться второй раз
            integrationApiMock.Verify(mock => mock.GetAbcpToEmexBrandMapping(), Times.Once());
        }

        /// <summary>
        /// Проверяет, что метод  GET: search-suggestions возвращает подсказки даже если сервис истории поиска недоступен
        /// </summary>
        [Fact]
        public async Task If_SearchHistory_Unavailable_Returns_Suggestions_Successfully()
        {
            var userId = 543254;

            var expectedSuggestions = new List<SearchSuggestion>();
            expectedSuggestions.Add(new SearchSuggestion
                { Brand = "Runway_Emex", Number = "010", Description = "Краска черная матовая (265г)" });
            expectedSuggestions.Add(new SearchSuggestion
                { Brand = "Febi", Number = "01089", Description = "Антифриз" });

            var apiFactory = new WebApplicationFactory<Startup>()
                .WithWebHostBuilder(builder =>
                {
                    builder.UseSetting("IsIntegrationTesting", "true");
                    builder.ConfigureServices(s =>
                    {
                        // IAuthLogic
                        var authLogicMock = new Mock<IAuthLogic>();
                        authLogicMock.Setup(m => m.AuthenticateAsync()).Returns(Task.FromResult(new UserAuth
                            { IsSuccess = true, Data = new UserDataWithVersion { UserId = userId } }));
                        s.AddScoped(s => authLogicMock.Object);

                        // ISearchHistoryApi - будет выбрасывать исключение
                        var searchHistoryApiMock = new Mock<ISearchHistoryApi>();
                        searchHistoryApiMock.Setup(m => m.GetUserSearchHistory(userId))
                            .Throws(new Exception("Любое исключение"));
                        s.AddScoped(s => searchHistoryApiMock.Object);

                        // ISuggestionsService
                        var suggestionsServiceMock = new Mock<ISuggestionsService>();
                        suggestionsServiceMock.Setup(m =>
                                m.GetSearchSuggestions(It.IsAny<string>()))
                            .Returns(Task.FromResult<IList<SearchSuggestion>>(expectedSuggestions));
                        s.AddScoped(s => suggestionsServiceMock.Object);
                    });
                });

            // Делаем запрос
            var client = apiFactory.CreateClient();
            var response = await client.GetAsync(new Uri(client.BaseAddress, "suggestions/search-suggestions?searchString=010"));

            // Проверяем что ответ 200
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Проверяем данные
            var data = JsonConvert.DeserializeObject<GetSearchSuggestionsWithSearchHistoryResponse>(
                await response.Content.ReadAsStringAsync());

            // История поиска должна быть пустой
            data.VinSearchHistory.Should().BeEmpty();
            data.DetailSearchHistory.Should().BeEmpty();

            // Подсказки должны присутствовать
            data.Suggestions.Should().BeEquivalentTo(expectedSuggestions);
        }
    }
}