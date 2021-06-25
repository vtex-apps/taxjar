namespace Taxjar.Models
{
    using Newtonsoft.Json;

    public class ErrorResponse
    {
        [JsonProperty("status")]
        public long Status { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("detail")]
        public string Detail { get; set; }
    }
}
