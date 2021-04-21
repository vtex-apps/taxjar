using System;
using System.Collections.Generic;
using System.Text;

namespace Taxjar.Models
{
    public class MerchantSettings
    {
        public string ApiToken { get; set; }
        public bool IsLive { get; set; }
        public bool EnableTransactionPosting { get; set; }
    }
}
