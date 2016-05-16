using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Rollbar {
    public class Data {
        private static readonly string NotifierAssemblyVersion = typeof(Data).Assembly.GetName().Version.ToString(3);
        public static string DefaultPlatform = "windows";
        public static string DefaultLanguage = "c#";

        public Data(string environment, Body body) {
            if (string.IsNullOrWhiteSpace(environment)) {
                throw new ArgumentNullException(nameof(environment));
            }
            if (body == null) {
                throw new ArgumentNullException(nameof(body));
            }
            Environment = environment;
            Body = body;
            Timestamp = (long) DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            Platform = DefaultPlatform;
            Language = DefaultLanguage;
        }

        [JsonProperty("environment", Required = Required.Always)]
        public string Environment { get; private set; }

        [JsonProperty("body", Required = Required.Always)]
        public Body Body { get; private set; }

        [JsonProperty("level", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public ErrorLevel? Level { get; set; }

        [JsonProperty("timestamp", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public long? Timestamp { get; set; }

        [JsonProperty("code_version", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string CodeVersion { get; set; }

        [JsonProperty("platform", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Platform { get; set; }

        [JsonProperty("language", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Language { get; set; }

        [JsonProperty("framework", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Framework { get; set; }

        [JsonProperty("context", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Context { get; set; }

        [JsonProperty("request", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Request Request { get; set; }

        [JsonProperty("person", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Person Person { get; set; }

        [JsonProperty("server", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Server Server { get; set; }

        [JsonProperty("client", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Client Client { get; set; }

        [JsonProperty("custom", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Dictionary<string, object> Custom { get; set; }

        [JsonProperty("fingerprint", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Fingerprint { get; set; }

        [JsonProperty("title", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Title { get; set; }

        [JsonProperty("uuid", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Uuid { get; set; }

        [JsonIgnore]
        public Guid? GuidUuid {
            get { return Uuid == null ? (Guid?) null : Guid.Parse(Uuid); }
            set { Uuid = value == null ? null : value.Value.ToString("N"); }
        }

        [JsonProperty("notifier", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Dictionary<string, string> Notifier {
            get {
                return new Dictionary<string, string> {
                    { "name", "Rollbar.NET" },
                    { "version", NotifierAssemblyVersion },
                };
            }
        }
    }
}
