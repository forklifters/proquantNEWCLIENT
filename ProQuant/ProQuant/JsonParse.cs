using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ProQuant
{
    public class JobsFromJson
    {
        public static List<Dictionary<string, string>> FromJson(string json) => JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(json, ProQuant.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this List<Dictionary<string, string>> self) => JsonConvert.SerializeObject(self, ProQuant.Converter.Settings);
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
