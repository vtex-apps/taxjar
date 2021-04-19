using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Taxjar.Models
{
    public class VtexTaxResponse
    {
        [JsonProperty("itemTaxResponse")]
        public ItemTaxResponse[] ItemTaxResponse { get; set; }

        [JsonProperty("hooks")]
        public Hook[] Hooks { get; set; }
    }

    public class Hook
    {
        [JsonProperty("major")]
        public long Major { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }
    }

    public class ItemTaxResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("taxes")]
        public VtexTax[] Taxes { get; set; }
    }

    public class VtexTax
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("value")]
        public decimal Value { get; set; }
    }
}