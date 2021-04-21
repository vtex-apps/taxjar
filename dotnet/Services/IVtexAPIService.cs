using System.Threading.Tasks;
using Taxjar.Models;

namespace Taxjar.Services
{
    public interface IVtexAPIService
    {
        Task<TaxForOrder> VtexRequestToTaxjarRequest(VtexTaxRequest vtexTaxRequest);
        Task<VtexTaxResponse> TaxjarResponseToVtexResponse(TaxResponse taxResponse);
        Task<VtexOrder> GetOrderInformation(string orderId);
        Task<string> InitConfiguration();
        Task<CreateTaxjarOrder> VtexOrderToTaxjarOrder(VtexOrder vtexOrder);
        Task<VtexDockResponse[]> ListVtexDocks();
    }
}