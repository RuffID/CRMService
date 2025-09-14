using CRMService.Interfaces.Api;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Text;

namespace CRMService.API
{
    public sealed class NewtonsoftJsonSerializer : IJsonSerializer
    {
        private static readonly Encoding Utf8 = new UTF8Encoding(true);
        private readonly JsonSerializerSettings _settings;
        public string MediaType => "application/json";

        public NewtonsoftJsonSerializer(JsonSerializerSettings? settings = null)
        {
            _settings = settings ?? new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                }
            };
        }

        public string Serialize(object value)
        {
            return JsonConvert.SerializeObject(value, _settings);
        }

        public T? Deserialize<T>(Stream stream, CancellationToken ct)
        {
            // Оставить stream открытым — владельцем распоряжается HttpApiClient
            using StreamReader sr = new (stream, Utf8, true, 8192, leaveOpen: true);
            using JsonTextReader reader = new (sr) { CloseInput = false };
            JsonSerializer serializer = JsonSerializer.Create(_settings);
            T? result = serializer.Deserialize<T>(reader);
            return result;
        }
    }
}