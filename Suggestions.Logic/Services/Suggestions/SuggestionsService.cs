using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Suggestions.Infrastructure.Services.Abcp.Model;
using Suggestions.Logic.Services.Abcp;
using Suggestions.Logic.Services.Suggestions.Model;

namespace Suggestions.Logic.Services.Suggestions
{
    public class SuggestionsService : ISuggestionsService
    {
        private readonly IAbcpService _abcpService;
        private readonly ILogger<SuggestionsService> _logger;

        public SuggestionsService(IAbcpService abcpService, ILogger<SuggestionsService> logger)
        {
            _abcpService = abcpService;
            _logger = logger;
        }

        public async Task<IList<SearchSuggestion>> GetSearchSuggestions(string searchString)
        {
            try
            {
                if (!_abcpService.Options.SuggestionsEnabled)
                {
                    return new List<SearchSuggestion>();
                }

                if (string.IsNullOrEmpty(searchString) || string.IsNullOrWhiteSpace(searchString))
                {
                    return new List<SearchSuggestion>();
                }

                var checkVINResult = CheckVIN(searchString);

                if (checkVINResult.IsVIN)
                {
                    return new List<SearchSuggestion>();
                }

                var checkFrameCodeResult = CheckFrameCode(searchString);
                if (checkFrameCodeResult.IsFrameCode)
                {
                    return new List<SearchSuggestion>();
                }
                
                var suggestions = await _abcpService.GetSearchSuggestions(searchString);

                // Заменяем бренды ABCP на бренды Emex
                var abcpToEmexMapping = await _abcpService.GetAbcpToEmexBrandMapping();
                var result = new List<SearchSuggestion>();
                foreach (var s in suggestions)
                    if (abcpToEmexMapping.TryGetValue(s.Brand, out var emexBrand))
                    {
                        s.Brand = emexBrand;
                        result.Add(new SearchSuggestion
                            { Brand = s.Brand, Description = s.Description, Number = s.Number });
                    }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в GetSearchSuggestions");
                return new List<SearchSuggestion>();
            }
        }

        public (bool IsVIN, string VIN) CheckVIN(string searchString)
        {
            if (searchString == null)
            {
                return (false, searchString);
            }

            searchString = searchString.Replace(" ", "");

            // Длина VIN - 17 символов
            if (searchString.Length != 17)
            {
                return (false, searchString);
            }

            // В номере VIN разрешено использовать только следующие символы: 0 1 2 3 4 5 6 7 8 9 A B C D E F G H J K L M N P R S T U V W X Y Z
            // VIS (от английского - Vehicle Indicator Section - часть номера кузова, идентифицирующая автомобиль) состоит из восьми знаков и замыкает VIN. Последние 4 знака обязательно должны быть цифрами.
            var regex = new Regex(@"^[A-HJ-NPR-Za-hj-npr-z\d]{13}\d{4}$");

            return (regex.IsMatch(searchString), searchString);
        }

        public (bool IsFrameCode, string FrameCodePart1, string FrameCodePart2) CheckFrameCode(string searchString)
        {
            if (searchString == null)
            {
                return (false, null, null);
            }

            searchString = searchString.Replace(" ", "");

            var frameCodeParts = searchString.Split('-');

            if (frameCodeParts.Length != 2)
            {
                return (false, null, null);
            }

            // TODO ...

            return (false, frameCodeParts[0], frameCodeParts[1]);
        }
    }
}