using Newtonsoft.Json;

namespace FacebookAuth.Models
{
    public class UserResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
