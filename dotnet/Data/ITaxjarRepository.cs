using Taxjar.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Taxjar.Data
{
    public interface ITaxjarRepository
    {
        Task<MerchantSettings> GetMerchantSettings();
    }
}