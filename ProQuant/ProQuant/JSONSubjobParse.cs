using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

public partial class SubJob
{
    [JsonProperty("job")]
    [JsonConverter(typeof(ParseStringConverter))]
    public int Job { get; set; }

    [JsonProperty("subjob")]
    [JsonConverter(typeof(ParseStringConverter))]
    public int Subjob { get; set; }

    [JsonProperty("status")]
    public string Status { get; set; }

    [JsonProperty("created")]
    public string Created { get; set; }

    [JsonProperty("sent")]
    [JsonConverter(typeof(ParseStringConverter))]
    public int Sent { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonProperty("netValue")]
    public string NetValue { get; set; }

    [JsonProperty("grossValue")]
    public string GrossValue { get; set; }

    [JsonProperty("vatValue")]
    public string VatValue { get; set; }
}

public partial class SubJob
{
    public static List<SubJob> FromJson(string json) => JsonConvert.DeserializeObject<List<SubJob>>(json, ProQuant.Converter.Settings);
}

public static class Serialize
{
    public static string ToJson(this List<SubJob> self) => JsonConvert.SerializeObject(self, ProQuant.Converter.Settings);
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

internal class ParseStringConverter : JsonConverter
{
    public override bool CanConvert(Type t) => t == typeof(long) || t == typeof(long?);

    public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null) return null;
        var value = serializer.Deserialize<string>(reader);
        long l;
        if (Int64.TryParse(value, out l))
        {
            return l;
        }
        throw new Exception("Cannot unmarshal type long");
    }

    public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
    {
        if (untypedValue == null)
        {
            serializer.Serialize(writer, null);
            return;
        }
        var value = (long)untypedValue;
        serializer.Serialize(writer, value.ToString());
        return;
    }

    public static readonly ParseStringConverter Singleton = new ParseStringConverter();
}

