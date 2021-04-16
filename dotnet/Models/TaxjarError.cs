using Newtonsoft.Json;

namespace Taxjar.Models
{
    public class TaxjarError
    {
        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("detail")]
        public string Detail { get; set; }

        [JsonProperty("status")]
        public string StatusCode { get; set; }
    }
}
