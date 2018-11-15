using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;


namespace ProQuant
{
    public partial class FirebaseJson
    {
        [JsonProperty("cmd")]
        public Cmd Cmd { get; set; }

        [JsonProperty("user")]
        public User User { get; set; }

        [JsonProperty("firebase")]
        public FirebaseProp Firebase { get; set; }
    }

    public partial class Cmd
    {
        [JsonProperty("command")]
        public string Command { get; set; }
    }

    public partial class FirebaseProp
    {
        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("device")]
        public string Device { get; set; }
    }

    public partial class User
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

    public partial class FirebaseJson
    {
        public static FirebaseJson FromJson(string json) => JsonConvert.DeserializeObject<FirebaseJson>(json, ConverterFirebase.Settings);
    }

    public static class SerializeFireBase
    {
        public static string ToJson(this FirebaseJson self) => JsonConvert.SerializeObject(self, ConverterFirebase.Settings);
    }

    internal static class ConverterFirebase
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}
