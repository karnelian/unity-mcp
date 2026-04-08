using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace KarnelLabs.MCP
{
    public static class JsonRpc
    {
        private static readonly JsonSerializerSettings Settings = new()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore,
        };

        /// <summary>
        /// rawId는 JSON 원본 표현 (숫자: "1", 문자열: "\"abc\"")
        /// TS가 숫자 id를 보내면 숫자로 응답해야 Map.get()이 매칭됨
        /// </summary>
        public static string Success(string rawId, object result)
        {
            var resultJson = JsonConvert.SerializeObject(result, Settings);
            return $"{{\"jsonrpc\":\"2.0\",\"id\":{rawId},\"result\":{resultJson}}}";
        }

        public static string Error(string rawId, int code, string message)
        {
            var errorJson = JsonConvert.SerializeObject(new { code, message }, Settings);
            return $"{{\"jsonrpc\":\"2.0\",\"id\":{rawId},\"error\":{errorJson}}}";
        }

        public class Request
        {
            [JsonProperty("jsonrpc")] public string Jsonrpc { get; set; }
            [JsonProperty("id")] public JToken Id { get; set; }
            [JsonProperty("method")] public string Method { get; set; }
            [JsonProperty("params")] public JToken Params { get; set; }

            /// <summary>
            /// Id를 JSON 원본 표현으로 반환 (숫자: "1", 문자열: "\"abc\"")
            /// </summary>
            public string GetIdRaw()
            {
                if (Id == null) return "null";
                return Id.ToString(Formatting.None);
            }
        }

        public static Request ParseRequest(string json)
        {
            return JsonConvert.DeserializeObject<Request>(json);
        }
    }
}
