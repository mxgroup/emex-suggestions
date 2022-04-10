using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Suggestions.Logic.UseCases.GetSearchSuggestionsForUnknownVisitor;
using Suggestions.Logic.UseCases.GetSearchSuggestionsWithGuestSearchHistory;
using Suggestions.Logic.UseCases.GetSearchSuggestionsWithUserSearchHistory;
using Suggestions.Logic.UseCases.Model;
using Suggestions.RestApi.Auth;

namespace Suggestions.RestApi.Controllers
{
    /// <summary>
    /// Работа с подсказками
    /// </summary>
    [ApiController]
    [Route("suggestions")]
    public class SuggestionsController : ControllerBase
    {
        private readonly IAuthLogic _authLogic;
        private readonly ISender _sender;

        public SuggestionsController(ISender sender, IAuthLogic authLogic)
        {
            _sender = sender;
            _authLogic = authLogic;
        }


        /// <summary>
        ///  Возвращает подсказки и историю поиска
        /// </summary>
        /// <param name="searchString" example="010"></param>
        /// <returns></returns>
        [HttpGet("search-suggestions")]
        public async Task<ActionResult<GetSearchSuggestionsWithSearchHistoryResponse>> GetSearchSuggestions(
            string searchString)
        {
            var res = await _authLogic.AuthenticateAsync();
            if (res.Data == null)
            {
                return StatusCode(500, new { Message = "Ошибка аутентификации", res.Error });
            }

            if (res.Data.UserId != 0)
            {
                return await _sender.Send(new GetSearchSuggestionsWithUserSearchHistoryRequest
                {
                    UserId = res.Data.UserId,
                    SearchString = searchString
                });
            }

            if (_authLogic.VisitorId != null)
            {
                return await _sender.Send(new GetSearchSuggestionsWithGuestSearchHistoryRequest
                {
                    GuestId = _authLogic.VisitorId.Value,
                    SearchString = searchString
                });
            }

            return await _sender.Send(new GetSearchSuggestionsForUnknownVisitorRequest
            {
                SearchString = searchString
            });
        }
    }
}