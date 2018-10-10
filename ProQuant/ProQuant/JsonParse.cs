using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ProQuant
{
    public partial class TokenInfo
    {
        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("md")]
        public string Md { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("temp")]
        public string Temp { get; set; }
    }

    public partial class TokenInfo
    {
        public static TokenInfo FromJson(string json) => JsonConvert.DeserializeObject<TokenInfo>(json, ProQuant.Converter.Settings);
    }

    public partial class PayLinkJsonParse
    {
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }
    }

    public partial class PayLinkJsonParse
    {
        public static PayLinkJsonParse FromJson(string json) => JsonConvert.DeserializeObject<PayLinkJsonParse>(json, ProQuant.Converter.Settings);
    }

    public partial class JobAmounts
    {
        [JsonProperty("jobs")]
        public string jobs { get; set; }

        [JsonProperty("subjobs")]
        public string subjobs { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }
    }

    public partial class JobAmounts
    {
        public static JobAmounts FromJson(string json) => JsonConvert.DeserializeObject<JobAmounts>(json, ProQuant.Converter.Settings);
    }

    public partial class RegisterResponse
    {
        [JsonProperty("message")]
        public string message { get; set; }

        [JsonProperty("error")]
        public string error { get; set; }
    }

    public partial class RegisterResponse
    {
        public static RegisterResponse FromJson(string json) => JsonConvert.DeserializeObject<RegisterResponse>(json, ProQuant.Converter.Settings);
    }

    public partial class SettingsObject
    {
        [JsonProperty("key")]
        public string key { get; set; }

        [JsonProperty("description")]
        public string description { get; set; }

        [JsonProperty("uom")]
        public string uom { get; set; }

        [JsonProperty("original")]
        public string original { get; set; }

        [JsonProperty("pqdefault")]
        public string pqdefault { get; set; }

        [JsonProperty("value")]
        public string value { get; set; }

        [JsonProperty("min")]
        public string min { get; set; }

        [JsonProperty("max")]
        public string max { get; set; }
    }

    public partial class SettingsObject
    {
        public static SettingsObject[] FromJson(string json) => JsonConvert.DeserializeObject<SettingsObject[]>(json, ProQuant.Converter.Settings);
    }

    public class JobsFromJson
    {
        public static List<Dictionary<string, string>> FromJson(string json) => JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(json, ProQuant.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this SettingsObject[] self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters = {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}
