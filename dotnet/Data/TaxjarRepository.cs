namespace Taxjar.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Taxjar.Models;
    using Taxjar.Services;
    using Microsoft.AspNetCore.Http;
    using Newtonsoft.Json;
    using Vtex.Api.Context;
    using Taxjar.Data;

    public class TaxjarRepository : ITaxjarRepository
    {
        private readonly IVtexEnvironmentVariableProvider _environmentVariableProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IIOServiceContext _context;
        private readonly ICachedKeys _cachedKeys;
        private readonly string _applicationName;

        public TaxjarRepository(IVtexEnvironmentVariableProvider environmentVariableProvider, IHttpContextAccessor httpContextAccessor, IHttpClientFactory clientFactory, IIOServiceContext context, ICachedKeys cachedKeys)
        {
            this._environmentVariableProvider = environmentVariableProvider ??
                                                throw new ArgumentNullException(nameof(environmentVariableProvider));

            this._httpContextAccessor = httpContextAccessor ??
                                        throw new ArgumentNullException(nameof(httpContextAccessor));

            this._clientFactory = clientFactory ??
                               throw new ArgumentNullException(nameof(clientFactory));

            this._context = context ??
                               throw new ArgumentNullException(nameof(context));

            this._cachedKeys = cachedKeys ??
                               throw new ArgumentNullException(nameof(cachedKeys));

            this._applicationName =
                $"{this._environmentVariableProvider.ApplicationVendor}.{this._environmentVariableProvider.ApplicationName}";
        }

        public async Task<MerchantSettings> GetMerchantSettings()
        {
            // Load merchant settings
            // 'http://apps.{{region}}.vtex.io/{{account}}/{{workspace}}/apps/{{appName}}/settings'
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"http://apps.{this._environmentVariableProvider.Region}.vtex.io/{this._httpContextAccessor.HttpContext.Request.Headers[TaxjarConstants.VTEX_ACCOUNT_HEADER_NAME]}/{this._httpContextAccessor.HttpContext.Request.Headers[TaxjarConstants.HEADER_VTEX_WORKSPACE]}/apps/{TaxjarConstants.APP_SETTINGS}/settings"),
            };

            string authToken = this._httpContextAccessor.HttpContext.Request.Headers[TaxjarConstants.HEADER_VTEX_CREDENTIAL];
            if (authToken != null)
            {
                request.Headers.Add(TaxjarConstants.AUTHORIZATION_HEADER_NAME, authToken);
            }

            var client = _clientFactory.CreateClient();
            var response = await client.SendAsync(request);
            string responseContent = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<MerchantSettings>(responseContent);
        }

        public async Task<string> GetOrderConfiguration()
        {
            // https://{{accountName}}.vtexcommercestable.com.br/api/checkout/pvt/configuration/orderForm
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"http://{this._httpContextAccessor.HttpContext.Request.Headers[TaxjarConstants.VTEX_ACCOUNT_HEADER_NAME]}.{TaxjarConstants.ENVIRONMENT}.com.br/api/checkout/pvt/configuration/orderForm"),
            };

            request.Headers.Add(TaxjarConstants.USE_HTTPS_HEADER_NAME, "true");
            string authToken = _context.Vtex.AdminUserAuthToken;
            if (authToken != null)
            {
                request.Headers.Add(TaxjarConstants.AUTHORIZATION_HEADER_NAME, authToken);
                request.Headers.Add(TaxjarConstants.VTEX_ID_HEADER_NAME, authToken);
            }

            var client = _clientFactory.CreateClient();
            var response = await client.SendAsync(request);
            string responseContent = await response.Content.ReadAsStringAsync();
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            // A helper method is in order for this as it does not return the stack trace etc.
            response.EnsureSuccessStatusCode();

            return responseContent;
        }

        public async Task<bool> SetOrderConfiguration(string jsonSerializedOrderConfig)
        {
            // https://{{accountName}}.vtexcommercestable.com.br/api/checkout/pvt/configuration/orderForm
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"http://{this._httpContextAccessor.HttpContext.Request.Headers[TaxjarConstants.VTEX_ACCOUNT_HEADER_NAME]}.{TaxjarConstants.ENVIRONMENT}.com.br/api/checkout/pvt/configuration/orderForm"),
                Content = new StringContent(jsonSerializedOrderConfig, Encoding.UTF8, TaxjarConstants.APPLICATION_JSON)
            };

            request.Headers.Add(TaxjarConstants.USE_HTTPS_HEADER_NAME, "true");
            string authToken = _context.Vtex.AdminUserAuthToken;
            if (authToken != null)
            {
                request.Headers.Add(TaxjarConstants.AUTHORIZATION_HEADER_NAME, authToken);
                request.Headers.Add(TaxjarConstants.VTEX_ID_HEADER_NAME, authToken);
            }

            var client = _clientFactory.CreateClient();
            var response = await client.SendAsync(request);
            string responseContent = await response.Content.ReadAsStringAsync();
            _context.Vtex.Logger.Info("SetOrderConfiguration", null, $"Request:\r{jsonSerializedOrderConfig}\rResponse: [{response.StatusCode}]\r{responseContent}");

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> SetSummaryRates(SummaryRatesStorage summaryRatesStorage)
        {
            var jsonSerializedProducReviews = JsonConvert.SerializeObject(summaryRatesStorage);
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri($"http://infra.io.vtex.com/vbase/v2/{this._httpContextAccessor.HttpContext.Request.Headers[TaxjarConstants.VTEX_ACCOUNT_HEADER_NAME]}/{this._httpContextAccessor.HttpContext.Request.Headers[TaxjarConstants.HEADER_VTEX_WORKSPACE]}/buckets/{this._applicationName}/{TaxjarConstants.BUCKET}/files/SummaryRates"),
                Content = new StringContent(jsonSerializedProducReviews, Encoding.UTF8, TaxjarConstants.APPLICATION_JSON)
            };

            string authToken = this._httpContextAccessor.HttpContext.Request.Headers[TaxjarConstants.HEADER_VTEX_CREDENTIAL];
            if (authToken != null)
            {
                request.Headers.Add(TaxjarConstants.AUTHORIZATION_HEADER_NAME, authToken);
                request.Headers.Add(TaxjarConstants.VTEX_ID_HEADER_NAME, authToken);
            }

            var client = _clientFactory.CreateClient();
            var response = await client.SendAsync(request);

            return response.IsSuccessStatusCode;
        }

        public async Task<SummaryRatesStorage> GetSummaryRates()
        {
            SummaryRatesStorage summaryRatesStorage = null;

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"http://infra.io.vtex.com/vbase/v2/{this._httpContextAccessor.HttpContext.Request.Headers[TaxjarConstants.VTEX_ACCOUNT_HEADER_NAME]}/{this._httpContextAccessor.HttpContext.Request.Headers[TaxjarConstants.HEADER_VTEX_WORKSPACE]}/buckets/{this._applicationName}/{TaxjarConstants.BUCKET}/files/SummaryRates")
            };

            string authToken = this._httpContextAccessor.HttpContext.Request.Headers[TaxjarConstants.HEADER_VTEX_CREDENTIAL];
            if (authToken != null)
            {
                request.Headers.Add(TaxjarConstants.AUTHORIZATION_HEADER_NAME, authToken);
                request.Headers.Add(TaxjarConstants.VTEX_ID_HEADER_NAME, authToken);
            }

            var client = _clientFactory.CreateClient();
            var response = await client.SendAsync(request);
            string responseContent = await response.Content.ReadAsStringAsync();
            if(response.IsSuccessStatusCode)
            {
                summaryRatesStorage = JsonConvert.DeserializeObject<SummaryRatesStorage>(responseContent);
            }

            return summaryRatesStorage;
        }

        public async Task<bool> CacheTaxResponse(VtexTaxResponse vtexTaxResponse, int cacheKey)
        {
            var jsonSerializedProducReviews = JsonConvert.SerializeObject(vtexTaxResponse);
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri($"http://infra.io.vtex.com/vbase/v2/{this._httpContextAccessor.HttpContext.Request.Headers[TaxjarConstants.VTEX_ACCOUNT_HEADER_NAME]}/{this._httpContextAccessor.HttpContext.Request.Headers[TaxjarConstants.HEADER_VTEX_WORKSPACE]}/buckets/{this._applicationName}/{TaxjarConstants.CACHE_BUCKET}/files/{cacheKey}"),
                Content = new StringContent(jsonSerializedProducReviews, Encoding.UTF8, TaxjarConstants.APPLICATION_JSON)
            };

            string authToken = this._httpContextAccessor.HttpContext.Request.Headers[TaxjarConstants.HEADER_VTEX_CREDENTIAL];
            if (authToken != null)
            {
                request.Headers.Add(TaxjarConstants.AUTHORIZATION_HEADER_NAME, authToken);
                request.Headers.Add(TaxjarConstants.VTEX_ID_HEADER_NAME, authToken);
            }

            var client = _clientFactory.CreateClient();
            var response = await client.SendAsync(request);

            return response.IsSuccessStatusCode;
        }

        public async Task<VtexTaxResponse> GetCachedTaxResponse(int cacheKey)
        {
            VtexTaxResponse vtexTaxResponse = null;

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"http://infra.io.vtex.com/vbase/v2/{this._httpContextAccessor.HttpContext.Request.Headers[TaxjarConstants.VTEX_ACCOUNT_HEADER_NAME]}/{this._httpContextAccessor.HttpContext.Request.Headers[TaxjarConstants.HEADER_VTEX_WORKSPACE]}/buckets/{this._applicationName}/{TaxjarConstants.CACHE_BUCKET}/files/{cacheKey}")
            };

            string authToken = this._httpContextAccessor.HttpContext.Request.Headers[TaxjarConstants.HEADER_VTEX_CREDENTIAL];
            if (authToken != null)
            {
                request.Headers.Add(TaxjarConstants.AUTHORIZATION_HEADER_NAME, authToken);
                request.Headers.Add(TaxjarConstants.VTEX_ID_HEADER_NAME, authToken);
            }

            var client = _clientFactory.CreateClient();
            var response = await client.SendAsync(request);
            string responseContent = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                vtexTaxResponse = JsonConvert.DeserializeObject<VtexTaxResponse>(responseContent);
            }

            return vtexTaxResponse;
        }

        public bool TryGetCache(int cacheKey, out VtexTaxResponse vtexTaxResponse)
        {
            bool success = false;
            vtexTaxResponse = null;
            try
            {
                vtexTaxResponse = GetCachedTaxResponse(cacheKey).Result;
                success = vtexTaxResponse != null;
            }
            catch (Exception ex)
            {
                _context.Vtex.Logger.Error("TryGetCache", null, "Error getting cache", ex);
            }

            return success;
        }

        public async Task<bool> SetCache(int cacheKey, VtexTaxResponse vtexTaxResponse)
        {
            bool success = false;

            try
            {
                success = await CacheTaxResponse(vtexTaxResponse, cacheKey);
                if(success)
                {
                    await _cachedKeys.AddCacheKey(cacheKey);
                }

                List<int> keysToRemove = await _cachedKeys.ListExpiredKeys();
                foreach (int cacheKeyToRemove in keysToRemove)
                {
                    await CacheTaxResponse(null, cacheKey);
                    await _cachedKeys.RemoveCacheKey(cacheKey);
                }
            }
            catch (Exception ex)
            {
                _context.Vtex.Logger.Error("TryGetCache", null, "Error setting cache", ex);
            }

            return success;
        }

        public async Task<bool> SetNexusRegions(NexusRegionsStorage nexusRegionsStorage)
        {
            var jsonSerializedProducReviews = JsonConvert.SerializeObject(nexusRegionsStorage);
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri($"http://infra.io.vtex.com/vbase/v2/{this._httpContextAccessor.HttpContext.Request.Headers[TaxjarConstants.VTEX_ACCOUNT_HEADER_NAME]}/{this._httpContextAccessor.HttpContext.Request.Headers[TaxjarConstants.HEADER_VTEX_WORKSPACE]}/buckets/{this._applicationName}/{TaxjarConstants.BUCKET}/files/NexusRegions"),
                Content = new StringContent(jsonSerializedProducReviews, Encoding.UTF8, TaxjarConstants.APPLICATION_JSON)
            };

            string authToken = this._httpContextAccessor.HttpContext.Request.Headers[TaxjarConstants.HEADER_VTEX_CREDENTIAL];
            if (authToken != null)
            {
                request.Headers.Add(TaxjarConstants.AUTHORIZATION_HEADER_NAME, authToken);
                request.Headers.Add(TaxjarConstants.VTEX_ID_HEADER_NAME, authToken);
            }

            var client = _clientFactory.CreateClient();
            var response = await client.SendAsync(request);

            return response.IsSuccessStatusCode;
        }

        public async Task<NexusRegionsStorage> GetNexusRegions()
        {
            NexusRegionsStorage nexusRegionsStorage = null;

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"http://infra.io.vtex.com/vbase/v2/{this._httpContextAccessor.HttpContext.Request.Headers[TaxjarConstants.VTEX_ACCOUNT_HEADER_NAME]}/{this._httpContextAccessor.HttpContext.Request.Headers[TaxjarConstants.HEADER_VTEX_WORKSPACE]}/buckets/{this._applicationName}/{TaxjarConstants.BUCKET}/files/NexusRegions")
            };

            string authToken = this._httpContextAccessor.HttpContext.Request.Headers[TaxjarConstants.HEADER_VTEX_CREDENTIAL];
            if (authToken != null)
            {
                request.Headers.Add(TaxjarConstants.AUTHORIZATION_HEADER_NAME, authToken);
                request.Headers.Add(TaxjarConstants.VTEX_ID_HEADER_NAME, authToken);
            }

            var client = _clientFactory.CreateClient();
            var response = await client.SendAsync(request);
            string responseContent = await response.Content.ReadAsStringAsync();
            if(response.IsSuccessStatusCode)
            {
                nexusRegionsStorage = JsonConvert.DeserializeObject<NexusRegionsStorage>(responseContent);
            }

            return nexusRegionsStorage;
        }
    }
}
