namespace Suggestions.RestApi.Auth
{
    public class UserAuth
    {
        public bool IsSuccess { get; set; }

        public string Error { get; set; }

        public UserDataWithVersion Data { get; set; }
    }
}