using System;
using Newtonsoft.Json;

namespace Suggestions.RestApi.Auth
{
    public static class JsonExtensions
    {
        public static T FromJson<T>(this string data)
            where T : class
        {
            return TryConvert(data, JsonConvert.DeserializeObject<T>);
        }

        public static string ToJson<T>(this T obj, bool ignoreNullValue = false)
            where T : class
        {
            var jsonSerializerSettings = ConfigureSettings(ignoreNullValue);

            return TryConvert(obj, data => JsonConvert.SerializeObject(data, jsonSerializerSettings));
        }

        private static TResult TryConvert<TData, TResult>(
            TData data,
            Func<TData, TResult> converter)
            where TResult : class
            where TData : class
        {
            TResult result = null;

            if (data != null)
            {
                result = converter(data);
            }

            return result;
        }

        private static JsonSerializerSettings ConfigureSettings(bool ignoreNullValue)
        {
            var settings = new JsonSerializerSettings();

            if (ignoreNullValue)
            {
                settings.NullValueHandling = NullValueHandling.Ignore;
            }

            return settings;
        }
    }
}