using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Taxjar.Models
{
    public class InvoiceHookOrderStatus
    {
        [JsonProperty("orderId")]
        public string OrderId { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }
    }
}
