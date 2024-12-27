using Newtonsoft.Json;

namespace MarketUp.Dtos
{
    public class CultureDto
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("shortName")]
        public string ShortName { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("number")]
        public string Number { get; set; }
    }
}
