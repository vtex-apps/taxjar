using System;
using System.Collections.Generic;
using System.Text;

namespace Taxjar.Models
{
    public class SummaryRatesStorage
    {
        public DateTime UpdatedAt { get; set; }
        public SummaryRatesResponse SummaryRatesResponse { get; set; }
    }
}
