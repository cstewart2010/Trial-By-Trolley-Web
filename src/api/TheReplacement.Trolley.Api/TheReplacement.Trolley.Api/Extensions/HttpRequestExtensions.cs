using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.IO;

namespace TheReplacement.Trolley.Api.Client.Extensions
{
    internal static class HttpRequestExtensions
    {
        public static T DeserializeBody<T>(this HttpRequest self) where T : class
        {
            var reader = new StreamReader(self.Body);
            var json = reader.ReadToEnd();
            var deserializedBody = JsonConvert.DeserializeObject<T>(json);
            return deserializedBody;
        }
    }
}
