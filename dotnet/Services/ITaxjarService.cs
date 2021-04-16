using System.Threading.Tasks;
using Taxjar.Models;

namespace Taxjar.Services
{
    public interface ITaxjarService
    {
        Task<CategoriesResponse> Categories();
        Task<RateResponse> RatesForLocation(string zip);
        Task<SummaryRatesResponse> SummaryRates();
        Task<TaxResponse> TaxForOrder(TaxForOrder taxForOrder);
    }
}