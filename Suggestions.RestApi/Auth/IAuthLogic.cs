using System;
using System.Threading.Tasks;

namespace Suggestions.RestApi.Auth
{
    public interface IAuthLogic
    {
        UserAuth UserAuth { get; }

        Guid? VisitorId { get; }

        Task<UserAuth> AuthenticateAsync();
    }
}