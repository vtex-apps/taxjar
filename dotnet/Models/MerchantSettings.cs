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
        public string TransactionPostingType { get; set; }
        public bool UseTaxJarNexus { get; set; }
        public string  SalesChannelExclude { get; set; }
    }
}
