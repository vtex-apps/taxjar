using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Taxjar.Models
{
    public class TaxFallbackResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("date")]
        public DateTimeOffset Date { get; set; }

        [JsonProperty("ZIP_CODE")]
        public string ZipCode { get; set; }

        [JsonProperty("STATE_ABBREV")]
        public string StateAbbrev { get; set; }

        [JsonProperty("COUNTY_NAME")]
        public string CountyName { get; set; }

        [JsonProperty("CITY_NAME")]
        public string CityName { get; set; }

        [JsonProperty("STATE_SALES_TAX")]
        public decimal StateSalesTax { get; set; }

        [JsonProperty("STATE_USE_TAX")]
        public decimal StateUseTax { get; set; }

        [JsonProperty("COUNTY_SALES_TAX")]
        public decimal CountySalesTax { get; set; }

        [JsonProperty("COUNTY_USE_TAX")]
        public decimal CountyUseTax { get; set; }

        [JsonProperty("CITY_SALES_TAX")]
        public long CitySalesTax { get; set; }

        [JsonProperty("CITY_USE_TAX")]
        public long CityUseTax { get; set; }

        [JsonProperty("TOTAL_SALES_TAX")]
        public decimal TotalSalesTax { get; set; }

        [JsonProperty("TOTAL_USE_TAX")]
        public decimal TotalUseTax { get; set; }

        [JsonProperty("TAX_SHIPPING_ALONE")]
        public bool TaxShippingAlone { get; set; }

        [JsonProperty("TAX_SHIPPING_AND_HANDLING_TOGETHER")]
        public bool TaxShippingAndHandlingTogether { get; set; }
    }
}
