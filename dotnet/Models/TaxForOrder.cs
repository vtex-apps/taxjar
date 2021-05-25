using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Taxjar.Models
{
    public class TaxForOrder
    {
        [JsonProperty("from_country")]
        public string FromCountry { get; set; }

        [JsonProperty("from_zip")]
        public string FromZip { get; set; }

        [JsonProperty("from_state")]
        public string FromState { get; set; }

        [JsonProperty("from_city")]
        public string FromCity { get; set; }

        [JsonProperty("from_street")]
        public string FromStreet { get; set; }

        [JsonProperty("to_country")]
        public string ToCountry { get; set; }

        [JsonProperty("to_zip")]
        public string ToZip { get; set; }

        [JsonProperty("to_state")]
        public string ToState { get; set; }

        [JsonProperty("to_city")]
        public string ToCity { get; set; }

        [JsonProperty("to_street")]
        public string ToStreet { get; set; }

        [JsonProperty("amount", NullValueHandling = NullValueHandling.Ignore)]
        public float Amount { get; set; }

        [JsonProperty("shipping")]
        public float Shipping { get; set; }

        [JsonProperty("sales_tax")]
        public float SalesTax { get; set; }

        [JsonProperty("customer_id")]
        public string CustomerId { get; set; }

        [JsonProperty("exemption_type", NullValueHandling = NullValueHandling.Ignore)]
        public string ExemptionType { get; set; }

        [JsonProperty("line_items")]
        public TaxForOrderLineItem[] LineItems { get; set; }

        [JsonProperty("nexus_addresses")]
        public TaxForOrderNexusAddress[] NexusAddresses { get; set; }

        [JsonProperty("plugin")]
        public string PlugIn { get; set; }
    }

    public class TaxForOrderLineItem
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("quantity")]
        public long Quantity { get; set; }

        [JsonProperty("product_tax_code")]
        public string ProductTaxCode { get; set; }

        [JsonProperty("unit_price")]
        public float UnitPrice { get; set; }

        [JsonProperty("discount")]
        public float Discount { get; set; }
    }

    public class TaxForOrderNexusAddress
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("zip")]
        public string Zip { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("street")]
        public string Street { get; set; }
    }
}

