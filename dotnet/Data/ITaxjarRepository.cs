using Taxjar.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Taxjar.Data
{
    public interface ITaxjarRepository
    {
        Task<MerchantSettings> GetMerchantSettings();
        Task<string> GetOrderConfiguration();
        Task<bool> SetOrderConfiguration(string jsonSerializedOrderConfig);

        Task<bool> SetSummaryRates(SummaryRatesStorage summaryRatesStorage);
        Task<SummaryRatesStorage> GetSummaryRates();
    }
}