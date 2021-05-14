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

        Task<OrdersResponse> ListOrders();
        Task<OrderResponse> ShowOrder(string transactionId);
        Task<OrderResponse> CreateOrder(CreateTaxjarOrder taxjarOrder);
        Task<OrderResponse> UpdateOrder(CreateTaxjarOrder taxjarOrder);
        Task<OrderResponse> DeleteOrder(string transactionId);

        Task<RefundsResponse> ListRefunds();
        Task<RefundResponse> ShowRefund(string transactionId);
        Task<RefundResponse> CreateRefund(CreateTaxjarOrder taxjarOrder);
        Task<RefundResponse> UpdateRefund(CreateTaxjarOrder taxjarOrder);
        Task<RefundResponse> DeleteRefund(string transactionId);

        Task<CustomersResponse> ListCustomers();
        Task<CustomerResponse> ShowCustomer(string customerId);
        Task<CustomerResponse> CreateCustomer(Customer taxjarCustomer);
        Task<CustomerResponse> UpdateCustomer(Customer taxjarCustomer);
        Task<CustomerResponse> DeleteCustomer(string customerId);

        Task<NexusRegionsResponse> ListNexusRegions();
    }
}