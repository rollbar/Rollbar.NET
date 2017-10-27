namespace Rollbar.DTOs
{
    using Newtonsoft.Json;

    public class Person
        : DtoBase
    {
        public Person(string id)
        {
            Id = id;
        }

        [JsonProperty("id", Required = Required.Always)]
        public string Id { get; set; }

        [JsonProperty("username", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string UserName { get; set; }

        [JsonProperty("email", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Email { get; set; }
    }
}
