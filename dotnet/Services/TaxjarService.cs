using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Razor;
using Newtonsoft.Json;
using Taxjar.Data;
using Taxjar.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Vtex.Api.Context;

namespace Taxjar.Services
{
    public class TaxjarService : ITaxjarService
    {
        private readonly IVtexEnvironmentVariableProvider _environmentVariableProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IIOServiceContext _context;
        private readonly ITaxjarRepository _taxjarRepository;
        private readonly string _applicationName;

        public TaxjarService(IVtexEnvironmentVariableProvider environmentVariableProvider, IHttpContextAccessor httpContextAccessor, IHttpClientFactory clientFactory, IIOServiceContext context, ITaxjarRepository taxjarRepository)
        {
            this._environmentVariableProvider = environmentVariableProvider ??
                                                throw new ArgumentNullException(nameof(environmentVariableProvider));

            this._httpContextAccessor = httpContextAccessor ??
                                        throw new ArgumentNullException(nameof(httpContextAccessor));

            this._clientFactory = clientFactory ??
                               throw new ArgumentNullException(nameof(clientFactory));

            this._context = context ??
                            throw new ArgumentNullException(nameof(context));

            this._taxjarRepository = taxjarRepository ??
                            throw new ArgumentNullException(nameof(taxjarRepository));

            this._applicationName =
                $"{this._environmentVariableProvider.ApplicationVendor}.{this._environmentVariableProvider.ApplicationName}";
        }

        public async Task<string> SendRequest(string endpoint, string jsonSerializedData, HttpMethod httpMethod)
        {
            string responseContent = null;

            MerchantSettings merchantSettings = await _taxjarRepository.GetMerchantSettings();

            try
            {
                var request = new HttpRequestMessage
                {
                    Method = httpMethod,
                    RequestUri = new Uri($"http://api.sandbox.taxjar.com/v2/{endpoint}")
                };

                if (!string.IsNullOrEmpty(jsonSerializedData))
                    request.Content = new StringContent(jsonSerializedData, Encoding.UTF8, TaxjarConstants.APPLICATION_JSON);

                request.Headers.Add(TaxjarConstants.AUTHORIZATION_HEADER_NAME, $"Bearer {merchantSettings.ApiToken}");
                request.Headers.Add(TaxjarConstants.USE_HTTPS_HEADER_NAME, "true");
                string authToken = this._httpContextAccessor.HttpContext.Request.Headers[TaxjarConstants.HEADER_VTEX_CREDENTIAL];
                if (authToken != null)
                {
                    request.Headers.Add(TaxjarConstants.VTEX_ID_HEADER_NAME, authToken);
                    request.Headers.Add(TaxjarConstants.PROXY_AUTHORIZATION_HEADER_NAME, authToken);
                }

                var client = _clientFactory.CreateClient();
                var response = await client.SendAsync(request);
                responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"SendRequest [{httpMethod}] {request.RequestUri}");
                //Console.WriteLine($"SendRequest [{jsonSerializedData}]");
                Console.WriteLine($"SendRequest [{response.StatusCode}] {responseContent}");
            }
            catch (Exception ex)
            {
                _context.Vtex.Logger.Error("SendRequest", null, $"Error sending to '{endpoint}'", ex);
            }

            return responseContent;
        }

        public async Task<CategoriesResponse> Categories()
        {
            CategoriesResponse categoriesResponse = null;
            string  response = await SendRequest("categories", null, HttpMethod.Get);
            if (!string.IsNullOrEmpty(response))
                categoriesResponse = JsonConvert.DeserializeObject<CategoriesResponse>(response);

            return categoriesResponse;
        }

        public async Task<RateResponse> RatesForLocation(string zip)
        {
            RateResponse rateResponse = null;
            string response = await SendRequest($"rates/{zip}", null, HttpMethod.Get);
            if(!string.IsNullOrEmpty(response))
                rateResponse = JsonConvert.DeserializeObject<RateResponse>(response);

            return rateResponse;
        }

        public async Task<SummaryRatesResponse> SummaryRates()
        {
            SummaryRatesResponse rateResponse = null;
            string response = await SendRequest($"summary_rates", null, HttpMethod.Get);
            if (!string.IsNullOrEmpty(response))
                rateResponse = JsonConvert.DeserializeObject<SummaryRatesResponse>(response);

            return rateResponse;
        }

        public async Task<TaxResponse> TaxForOrder(TaxForOrder taxForOrder)
        {
            TaxResponse taxResponse = null;
            string jsonSerialiezedData = JsonConvert.SerializeObject(taxForOrder);
            string response = await SendRequest($"taxes", jsonSerialiezedData, HttpMethod.Post);
            if (!string.IsNullOrEmpty(response))
                taxResponse = JsonConvert.DeserializeObject<TaxResponse>(response);

            return taxResponse;
        }

        public async Task<OrdersResponse> ListOrders()
        {
            OrdersResponse ordersResponse = null;
            string response = await SendRequest($"transactions/orders", null, HttpMethod.Get);
            if (!string.IsNullOrEmpty(response))
                ordersResponse = JsonConvert.DeserializeObject<OrdersResponse>(response);

            return ordersResponse;
        }

        public async Task<OrderResponse> ShowOrder(string transactionId)
        {
            OrderResponse orderResponse = null;
            string response = await SendRequest($"transactions/orders/{transactionId}", null, HttpMethod.Get);
            if (!string.IsNullOrEmpty(response))
                orderResponse = JsonConvert.DeserializeObject<OrderResponse>(response);

            return orderResponse;
        }

        
    }
}
