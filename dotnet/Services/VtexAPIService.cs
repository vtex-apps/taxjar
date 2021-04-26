using Taxjar.Data;
using Taxjar.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Vtex.Api.Context;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Taxjar.Services
{
    public class VtexAPIService : IVtexAPIService
    {
        private readonly IIOServiceContext _context;
        private readonly IVtexEnvironmentVariableProvider _environmentVariableProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHttpClientFactory _clientFactory;
        private readonly ITaxjarRepository _taxjarRepository;
        private readonly string _applicationName;

        public VtexAPIService(IIOServiceContext context, IVtexEnvironmentVariableProvider environmentVariableProvider, IHttpContextAccessor httpContextAccessor, IHttpClientFactory clientFactory, ITaxjarRepository taxjarRepository)
        {
            this._context = context ??
                            throw new ArgumentNullException(nameof(context));

            this._environmentVariableProvider = environmentVariableProvider ??
                                                throw new ArgumentNullException(nameof(environmentVariableProvider));

            this._httpContextAccessor = httpContextAccessor ??
                                        throw new ArgumentNullException(nameof(httpContextAccessor));

            this._clientFactory = clientFactory ??
                               throw new ArgumentNullException(nameof(clientFactory));

            this._taxjarRepository = taxjarRepository ??
                               throw new ArgumentNullException(nameof(taxjarRepository));

            this._applicationName =
                $"{this._environmentVariableProvider.ApplicationVendor}.{this._environmentVariableProvider.ApplicationName}";
        }

        public async Task<TaxForOrder> VtexRequestToTaxjarRequest(VtexTaxRequest vtexTaxRequest)
        {
            VtexDockResponse[] vtexDocks = await this.ListVtexDocks();
            if(vtexDocks == null)
            {
                Console.WriteLine("Could not load docks");
                _context.Vtex.Logger.Error("VtexRequestToTaxjarRequest", null, "Could not load docks.");
                return null;
            }

            string dockId = vtexTaxRequest.Items.Select(i => i.DockId).FirstOrDefault();
            Console.WriteLine($"DOCK = '{dockId}'");
            //VtexDockResponse vtexDock = await this.ListDockById(dockId);
            VtexDockResponse vtexDock = vtexDocks.Where(d => d.Id.Equals(dockId)).FirstOrDefault();
            if(vtexDock == null)
            {
                Console.WriteLine($"Dock '{dockId}' not found.");
                _context.Vtex.Logger.Error("VtexRequestToTaxjarRequest", null, $"Dock '{dockId}' not found.");
                return null;
            }

            TaxForOrder taxForOrder = new TaxForOrder
            {
                //Amount = (float)vtexTaxRequest.Totals.Sum(t => t.Value) / 100,
                Shipping = (float)vtexTaxRequest.Totals.Where(t => t.Id.Equals("Shipping")).Sum(t => t.Value) / 100,
                ToCity = vtexTaxRequest.ShippingDestination.City,
                ToCountry = vtexTaxRequest.ShippingDestination.Country.Substring(0, 2),
                ToState = vtexTaxRequest.ShippingDestination.State,
                ToStreet = vtexTaxRequest.ShippingDestination.Street,
                ToZip = vtexTaxRequest.ShippingDestination.PostalCode,
                FromCity = vtexDock.PickupStoreInfo.Address.City,
                FromCountry = vtexDock.PickupStoreInfo.Address.Country.Acronym.Substring(0, 2),
                FromState = vtexDock.PickupStoreInfo.Address.State,
                FromStreet = vtexDock.PickupStoreInfo.Address.Street,
                FromZip = vtexDock.PickupStoreInfo.Address.PostalCode,
                CustomerId = vtexTaxRequest.ClientEmail,
                LineItems = new TaxForOrderLineItem[vtexTaxRequest.Items.Length],
                //ExemptionType = TaxjarConstants.ExemptionType.NON_EXEMPT
            };

            for (int i = 0; i < vtexTaxRequest.Items.Length; i++)
            {
                string taxCode = null;
                GetSkuContextResponse skuContextResponse = await this.GetSku(vtexTaxRequest.Items[i].Id);
                if(skuContextResponse != null)
                {
                    taxCode = skuContextResponse.TaxCode;
                }

                taxForOrder.LineItems[i] = new TaxForOrderLineItem
                {
                    Discount = (float)vtexTaxRequest.Items[i].DiscountPrice,
                    Id = vtexTaxRequest.Items[i].Id,
                    ProductTaxCode = taxCode,
                    Quantity = vtexTaxRequest.Items[i].Quantity,
                    UnitPrice = (float)(vtexTaxRequest.Items[i].ItemPrice / vtexTaxRequest.Items[i].Quantity)
                };
            }

            List<TaxForOrderNexusAddress> nexuses = new List<TaxForOrderNexusAddress>();
            foreach(VtexDockResponse dock in vtexDocks)
            {
                if (dock.PickupStoreInfo.Address != null)
                {
                    nexuses.Add(
                            new TaxForOrderNexusAddress
                            {
                                City = dock.PickupStoreInfo.Address.City,
                                Country = string.IsNullOrEmpty(dock.PickupStoreInfo.Address.Country.Acronym) ? string.Empty : dock.PickupStoreInfo.Address.Country.Acronym.Substring(0, 2),
                                Id = dock.Id,
                                State = dock.PickupStoreInfo.Address.State,
                                Street = dock.PickupStoreInfo.Address.Street,
                                Zip = dock.PickupStoreInfo.Address.PostalCode
                            }
                        );
                }
                else
                {
                    _context.Vtex.Logger.Warn("VtexRequestToTaxjarRequest", null, $"Dock {dock.Id} missing address");
                    Console.WriteLine($"Dock {dock.Id} missing address");
                }
            }

            taxForOrder.NexusAddresses = nexuses.ToArray();

            _context.Vtex.Logger.Info("VtexRequestToTaxjarRequest", null, $"Request: {JsonConvert.SerializeObject(vtexTaxRequest)}\nResponse: {JsonConvert.SerializeObject(taxForOrder)}");

            return taxForOrder;
        }

        public async Task<VtexTaxResponse> TaxjarResponseToVtexResponse(TaxResponse taxResponse)
        {
            if(taxResponse == null)
            {
                Console.WriteLine("taxResponse is null");
                return null;
            }

            if (taxResponse.Tax == null)
            {
                Console.WriteLine("Tax is null");
                return null;
            }

            if (taxResponse.Tax.Breakdown == null)
            {
                Console.WriteLine("Breakdown is null");
                return null;
            }

            VtexTaxResponse vtexTaxResponse = new VtexTaxResponse
            {
                Hooks = new Hook[]
                {
                    new Hook
                    {
                        //Major = 1,
                        //Url = new Uri($"https://{this._httpContextAccessor.HttpContext.Request.Headers[TaxjarConstants.HEADER_VTEX_WORKSPACE]}--{this._httpContextAccessor.HttpContext.Request.Headers[TaxjarConstants.VTEX_ACCOUNT_HEADER_NAME]}.myvtex.com/taxjar/oms/invoice")
                    }
                },
                ItemTaxResponse = new ItemTaxResponse[taxResponse.Tax.Breakdown.LineItems.Count]
            };

            double shippingTaxCity = (double)taxResponse.Tax.Breakdown.Shipping.CityAmount;
            double shippingTaxCounty = (double)taxResponse.Tax.Breakdown.Shipping.CountyAmount;
            double shippingTaxSpecial = (double)taxResponse.Tax.Breakdown.Shipping.SpecialDistrictAmount;
            double shippingTaxState = (double)taxResponse.Tax.Breakdown.Shipping.StateAmount;
            double totalItemTax = (double)taxResponse.Tax.Breakdown.LineItems.Sum(i => i.TaxCollectable);
            //double itemTaxPercentOfWhole = 1D / (double)taxResponse.Tax.Breakdown.LineItems.Count;
            //Console.WriteLine($"itemTaxPercentOfWhole = {itemTaxPercentOfWhole}%");

            for (int i = 0; i < taxResponse.Tax.Breakdown.LineItems.Count; i++)
            {
                TaxBreakdownLineItem lineItem = taxResponse.Tax.Breakdown.LineItems[i];
                double itemTaxPercentOfWhole = (double)lineItem.TaxCollectable / totalItemTax;
                ItemTaxResponse itemTaxResponse = new ItemTaxResponse
                {
                    Id = lineItem.Id
                };

                List<VtexTax> vtexTaxes = new List<VtexTax>();
                if (lineItem.StateAmount > 0)
                {
                    vtexTaxes.Add(
                        new VtexTax
                        {
                            Description = "",
                            Name = $"STATE TAX: {taxResponse.Tax.Jurisdictions.State}", // NY COUNTY TAX: MONROE
                            Value = lineItem.StateAmount
                        }
                     );
                }

                if (lineItem.CountyAmount > 0)
                {
                    vtexTaxes.Add(
                        new VtexTax
                        {
                            Description = "",
                            Name = $"COUNTY TAX: {taxResponse.Tax.Jurisdictions.County}",
                            Value = lineItem.CountyAmount
                        }
                     );
                }

                if (lineItem.CityAmount > 0)
                {
                    vtexTaxes.Add(
                        new VtexTax
                        {
                            Description = "",
                            Name = $"CITY TAX: {taxResponse.Tax.Jurisdictions.City}",
                            Value = lineItem.CityAmount
                        }
                     );
                }

                if (lineItem.SpecialDistrictAmount > 0)
                {
                    vtexTaxes.Add(
                        new VtexTax
                        {
                            Description = "",
                            Name = "SPECIAL TAX",
                            Value = lineItem.SpecialDistrictAmount
                        }
                     );
                }

                if (shippingTaxState > 0)
                {
                    Console.WriteLine($"item[{i}] shippingTaxState: {shippingTaxState}*{itemTaxPercentOfWhole}={(decimal)Math.Round(shippingTaxState * itemTaxPercentOfWhole, 2, MidpointRounding.ToEven)}");
                    vtexTaxes.Add(
                        new VtexTax
                        {
                            Description = "",
                            Name = $"STATE TAX: {taxResponse.Tax.Jurisdictions.State} (SHIPPING)",
                            Value = (decimal)Math.Round(shippingTaxState * itemTaxPercentOfWhole, 2, MidpointRounding.ToEven)
                        }
                     );
                }

                if (shippingTaxCounty > 0)
                {
                    Console.WriteLine($"item[{i}] shippingTaxCounty: {shippingTaxCounty}*{itemTaxPercentOfWhole}={(decimal)Math.Round(shippingTaxCounty * itemTaxPercentOfWhole, 2, MidpointRounding.ToEven)}");
                    vtexTaxes.Add(
                        new VtexTax
                        {
                            Description = "",
                            Name = $"COUNTY TAX: {taxResponse.Tax.Jurisdictions.County} (SHIPPING)",
                            Value = (decimal)Math.Round(shippingTaxCounty * itemTaxPercentOfWhole, 2, MidpointRounding.ToEven)
                        }
                     );
                }

                if (shippingTaxCity > 0)
                {
                    Console.WriteLine($"item[{i}] shippingTaxCity: {shippingTaxCity}*{itemTaxPercentOfWhole}={(decimal)Math.Round(shippingTaxCity * itemTaxPercentOfWhole, 2, MidpointRounding.ToEven)}");
                    vtexTaxes.Add(
                        new VtexTax
                        {
                            Description = "",
                            Name = $"CITY TAX: {taxResponse.Tax.Jurisdictions.City} (SHIPPING)",
                            Value = (decimal)Math.Round(shippingTaxCity * itemTaxPercentOfWhole, 2, MidpointRounding.ToEven)
                        }
                     );
                }

                if (shippingTaxSpecial > 0)
                {
                    Console.WriteLine($"item[{i}] shippingTaxSpecial: {shippingTaxSpecial}*{itemTaxPercentOfWhole}={(decimal)Math.Round(shippingTaxSpecial * itemTaxPercentOfWhole, 2, MidpointRounding.ToEven)}");
                    vtexTaxes.Add(
                        new VtexTax
                        {
                            Description = "",
                            Name = "SPECIAL TAX (SHIPPING)",
                            Value = (decimal)Math.Round(shippingTaxSpecial * itemTaxPercentOfWhole, 2, MidpointRounding.ToEven)
                        }
                     );
                }

                itemTaxResponse.Taxes = vtexTaxes.ToArray();
                vtexTaxResponse.ItemTaxResponse[i] = itemTaxResponse;
            };

            decimal totalOrderTax = (decimal)taxResponse.Tax.AmountToCollect;
            decimal totalResponseTax = vtexTaxResponse.ItemTaxResponse.SelectMany(t => t.Taxes).Sum(i => i.Value);
            if(!totalOrderTax.Equals(totalResponseTax))
            {
                Console.WriteLine($"Order Tax = {totalOrderTax} =/= Response Tax = {totalResponseTax}");
                decimal adjustmentAmount = Math.Round((totalOrderTax - totalResponseTax),2,MidpointRounding.ToEven);
                Console.WriteLine($"Adjustment = {adjustmentAmount}");
                int lastItemIndex = vtexTaxResponse.ItemTaxResponse.Length - 1;
                int lastTaxIndex = vtexTaxResponse.ItemTaxResponse[lastItemIndex].Taxes.Length - 1;
                vtexTaxResponse.ItemTaxResponse[lastItemIndex].Taxes[lastTaxIndex].Value += adjustmentAmount;
                _context.Vtex.Logger.Info("TaxjarResponseToVtexResponse", null, $"Applying adjustment: {totalOrderTax} - {totalResponseTax} = {adjustmentAmount}");
            }
            else
            {
                Console.WriteLine($"Order Tax = {totalOrderTax} == Response Tax = {totalResponseTax}");
            }

            _context.Vtex.Logger.Info("TaxjarResponseToVtexResponse", null, $"Request: {JsonConvert.SerializeObject(taxResponse)}\nResponse: {JsonConvert.SerializeObject(vtexTaxResponse)}");

            return vtexTaxResponse;
        }

        public async Task<CreateTaxjarOrder> VtexOrderToTaxjarOrder(VtexOrder vtexOrder)
        {
            CreateTaxjarOrder taxjarOrder = new CreateTaxjarOrder
            {
                TransactionId = vtexOrder.OrderId,
                TransactionDate = DateTime.Parse(vtexOrder.InvoicedDate).ToString("yyyy-MM-ddTHH:mm:sszzz"),
                Provider = vtexOrder.SalesChannel,
                ToCountry = vtexOrder.ShippingData.Address.Country.Substring(0, 2),
                ToZip = vtexOrder.ShippingData.Address.PostalCode,
                ToState = vtexOrder.ShippingData.Address.State,
                ToCity = vtexOrder.ShippingData.Address.City,
                ToStreet = vtexOrder.ShippingData.Address.Street,
                Amount = (decimal)vtexOrder.Totals.Where(t => !t.Id.Contains("Tax")).Sum(t => t.Value) / 100,
                Shipping = (decimal)vtexOrder.Totals.Where(t => t.Id.Contains("Shipping")).Sum(t => t.Value) / 100,
                SalesTax = (decimal)vtexOrder.Totals.Where(t => t.Id.Contains("Tax")).Sum(t => t.Value) / 100,
                CustomerId = vtexOrder.ClientProfileData.Email,
                //ExemptionType = TaxjarConstants.ExemptionType.NON_EXEMPT,
                LineItems = new List<LineItem>()
            };

            Console.WriteLine($"'{taxjarOrder.TransactionId}' Amount: {taxjarOrder.Amount} Shipping: {taxjarOrder.Shipping} SalesTax: {taxjarOrder.SalesTax} ");

            // For now, get the first address - TODO: split order by location
            LogisticsInfo logisticsInfo = vtexOrder.ShippingData.LogisticsInfo.FirstOrDefault();
            if (logisticsInfo != null)
            {
                DeliveryId deliveryId = new DeliveryId();
                if (logisticsInfo.DeliveryIds != null)
                {
                    deliveryId = logisticsInfo.DeliveryIds.FirstOrDefault();
                    VtexDockResponse vtexDock = await this.ListDockById(deliveryId.DockId);
                    if (vtexDock != null && vtexDock.PickupStoreInfo != null && vtexDock.PickupStoreInfo.Address != null)
                    {
                        taxjarOrder.FromCountry = vtexDock.PickupStoreInfo.Address.Country.Acronym.Substring(0, 2);
                        taxjarOrder.FromZip = vtexDock.PickupStoreInfo.Address.PostalCode;
                        taxjarOrder.FromState = vtexDock.PickupStoreInfo.Address.State;
                        taxjarOrder.FromCity = vtexDock.PickupStoreInfo.Address.City;
                        taxjarOrder.FromStreet = vtexDock.PickupStoreInfo.Address.Street;
                    }
                }
            }

            foreach (VtexOrderItem vtexOrderItem in vtexOrder.Items)
            {
                GetSkuContextResponse contextResponse = await this.GetSku(vtexOrderItem.SellerSku);

                LineItem taxForOrderLineItem = new LineItem
                {
                    Id = vtexOrderItem.Id,
                    Quantity = (int)vtexOrderItem.Quantity,
                    ProductIdentifier = vtexOrderItem.SellerSku,
                    Description = vtexOrderItem.Name,
                    ProductTaxCode = contextResponse.TaxCode,
                    UnitPrice = (decimal)vtexOrderItem.Price / 100,
                    Discount = Math.Abs((decimal)vtexOrderItem.PriceTags.Where(p => p.Name.Contains("DISCOUNT@")).Sum(p => p.Value) / 100),
                    SalesTax = (decimal)vtexOrderItem.PriceTags.Where(t => t.Name.Contains("TAX")).Sum(t => t.Value) / 100
                };

                taxjarOrder.LineItems.Add(taxForOrderLineItem);
                Console.WriteLine($"'{taxjarOrder.TransactionId}' [{taxForOrderLineItem.Description}] UnitPrice: {taxForOrderLineItem.UnitPrice} Discount: {taxForOrderLineItem.Discount} SalesTax: {taxForOrderLineItem.SalesTax} ");
            }

            return taxjarOrder;
        }

        public async Task<VtexOrder> GetOrderInformation(string orderId)
        {
            //Console.WriteLine("------- Headers -------");
            //foreach (var header in this._httpContextAccessor.HttpContext.Request.Headers)
            //{
            //    Console.WriteLine($"{header.Key}: {header.Value}");
            //}
            //Console.WriteLine($"http://{this._httpContextAccessor.HttpContext.Request.Headers[Constants.VTEX_ACCOUNT_HEADER_NAME]}.{Constants.ENVIRONMENT}.com.br/api/checkout/pvt/orders/{orderId}");

            VtexOrder vtexOrder = null;

            try
            {
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri($"http://{this._httpContextAccessor.HttpContext.Request.Headers[TaxjarConstants.VTEX_ACCOUNT_HEADER_NAME]}.{TaxjarConstants.ENVIRONMENT}.com.br/api/oms/pvt/orders/{orderId}")
                };

                request.Headers.Add(TaxjarConstants.USE_HTTPS_HEADER_NAME, "true");
                //request.Headers.Add(Constants.ACCEPT, Constants.APPLICATION_JSON);
                //request.Headers.Add(Constants.CONTENT_TYPE, Constants.APPLICATION_JSON);
                string authToken = this._httpContextAccessor.HttpContext.Request.Headers[TaxjarConstants.HEADER_VTEX_CREDENTIAL];
                //Console.WriteLine($"Token = '{authToken}'");
                if (authToken != null)
                {
                    request.Headers.Add(TaxjarConstants.AUTHORIZATION_HEADER_NAME, authToken);
                    request.Headers.Add(TaxjarConstants.VTEX_ID_HEADER_NAME, authToken);
                    request.Headers.Add(TaxjarConstants.PROXY_AUTHORIZATION_HEADER_NAME, authToken);
                }

                //StringBuilder sb = new StringBuilder();

                var client = _clientFactory.CreateClient();
                var response = await client.SendAsync(request);
                string responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    vtexOrder = JsonConvert.DeserializeObject<VtexOrder>(responseContent);
                    Console.WriteLine($"GetOrderInformation: [{response.StatusCode}] ");
                }
                else
                {
                    Console.WriteLine($"GetOrderInformation: [{response.StatusCode}] '{responseContent}'");
                    _context.Vtex.Logger.Info("GetOrderInformation", null, $"Order# {orderId} [{response.StatusCode}] '{responseContent}'");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetOrderInformation Error: {ex.Message}");
                _context.Vtex.Logger.Error("GetOrderInformation", null, $"Order# {orderId} Error", ex);
            }

            return vtexOrder;
        }

        public async Task<VtexDockResponse[]> ListVtexDocks()
        {
            VtexDockResponse[] listVtexDocks = null;
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"http://{this._httpContextAccessor.HttpContext.Request.Headers[TaxjarConstants.VTEX_ACCOUNT_HEADER_NAME]}.vtexcommercestable.com.br/api/logistics/pvt/configuration/docks")
            };

            request.Headers.Add(TaxjarConstants.USE_HTTPS_HEADER_NAME, "true");
            string authToken = this._httpContextAccessor.HttpContext.Request.Headers[TaxjarConstants.HEADER_VTEX_CREDENTIAL];
            if (authToken != null)
            {
                request.Headers.Add(TaxjarConstants.AUTHORIZATION_HEADER_NAME, authToken);
                request.Headers.Add(TaxjarConstants.VTEX_ID_HEADER_NAME, authToken);
                request.Headers.Add(TaxjarConstants.PROXY_AUTHORIZATION_HEADER_NAME, authToken);
            }

            //MerchantSettings merchantSettings = await _shipStationRepository.GetMerchantSettings();
            //request.Headers.Add(TaxjarConstants.APP_KEY, merchantSettings.AppKey);
            //request.Headers.Add(TaxjarConstants.APP_TOKEN, merchantSettings.AppToken);

            var client = _clientFactory.CreateClient();
            var response = await client.SendAsync(request);
            string responseContent = await response.Content.ReadAsStringAsync();
            //Console.WriteLine($"ListVtexDocks [{response.StatusCode}] {responseContent}");
            Console.WriteLine($"ListVtexDocks [{response.StatusCode}] ");
            if (response.IsSuccessStatusCode)
            {
                listVtexDocks = JsonConvert.DeserializeObject<VtexDockResponse[]>(responseContent);
            }

            return listVtexDocks;
        }

        public async Task<VtexDockResponse> ListDockById(string dockId)
        {
            VtexDockResponse dockResponse = null;
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"http://{this._httpContextAccessor.HttpContext.Request.Headers[TaxjarConstants.VTEX_ACCOUNT_HEADER_NAME]}.vtexcommercestable.com.br/api/logistics/pvt/configuration/docks/{dockId}")
            };

            request.Headers.Add(TaxjarConstants.USE_HTTPS_HEADER_NAME, "true");
            string authToken = this._httpContextAccessor.HttpContext.Request.Headers[TaxjarConstants.HEADER_VTEX_CREDENTIAL];
            if (authToken != null)
            {
                request.Headers.Add(TaxjarConstants.AUTHORIZATION_HEADER_NAME, authToken);
                request.Headers.Add(TaxjarConstants.VTEX_ID_HEADER_NAME, authToken);
                request.Headers.Add(TaxjarConstants.PROXY_AUTHORIZATION_HEADER_NAME, authToken);
            }

            //MerchantSettings merchantSettings = await _shipStationRepository.GetMerchantSettings();
            //request.Headers.Add(TaxjarConstants.APP_KEY, merchantSettings.AppKey);
            //request.Headers.Add(TaxjarConstants.APP_TOKEN, merchantSettings.AppToken);

            var client = _clientFactory.CreateClient();
            var response = await client.SendAsync(request);
            string responseContent = await response.Content.ReadAsStringAsync();
            //Console.WriteLine($"ListDockById [{response.StatusCode}] {responseContent}");
            Console.WriteLine($"ListDockById [{response.StatusCode}] ");
            if (response.IsSuccessStatusCode)
            {
                dockResponse = JsonConvert.DeserializeObject<VtexDockResponse>(responseContent);
            }

            return dockResponse;
        }

        public async Task<GetSkuContextResponse> GetSku(string skuId)
        {
            // GET https://{accountName}.{environment}.com.br/api/catalog_system/pvt/sku/stockkeepingunitbyid/skuId

            GetSkuContextResponse getSkuResponse = null;

            try
            {
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri($"http://{this._httpContextAccessor.HttpContext.Request.Headers[TaxjarConstants.VTEX_ACCOUNT_HEADER_NAME]}.{TaxjarConstants.ENVIRONMENT}.com.br/api/catalog_system/pvt/sku/stockkeepingunitbyid/{skuId}")
                };

                request.Headers.Add(TaxjarConstants.USE_HTTPS_HEADER_NAME, "true");
                string authToken = this._httpContextAccessor.HttpContext.Request.Headers[TaxjarConstants.HEADER_VTEX_CREDENTIAL];
                if (authToken != null)
                {
                    request.Headers.Add(TaxjarConstants.AUTHORIZATION_HEADER_NAME, authToken);
                    request.Headers.Add(TaxjarConstants.VTEX_ID_HEADER_NAME, authToken);
                    request.Headers.Add(TaxjarConstants.PROXY_AUTHORIZATION_HEADER_NAME, authToken);
                }

                var client = _clientFactory.CreateClient();
                var response = await client.SendAsync(request);
                string responseContent = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    getSkuResponse = JsonConvert.DeserializeObject<GetSkuContextResponse>(responseContent);
                }
                else
                {
                    _context.Vtex.Logger.Warn("GetSku", null, $"Did not get context for skuid '{skuId}'");
                }
            }
            catch (Exception ex)
            {
                _context.Vtex.Logger.Error("GetSku", null, $"Error getting context for skuid '{skuId}'", ex);
            }

            return getSkuResponse;
        }

        public async Task<string> InitConfiguration()
        {
            string retval = string.Empty;
            string jsonSerializedOrderConfig = await this._taxjarRepository.GetOrderConfiguration();
            if (string.IsNullOrEmpty(jsonSerializedOrderConfig))
            {
                retval = "Could not load Order Configuration.";
            }
            else
            {
                dynamic orderConfig = JsonConvert.DeserializeObject(jsonSerializedOrderConfig);
                VtexOrderformTaxConfiguration taxConfiguration = new VtexOrderformTaxConfiguration
                {
                    AllowExecutionAfterErrors = false,
                    IntegratedAuthentication = true,
                    Url = $"https://{this._httpContextAccessor.HttpContext.Request.Headers[TaxjarConstants.HEADER_VTEX_WORKSPACE]}--{this._httpContextAccessor.HttpContext.Request.Headers[TaxjarConstants.HEADER_VTEX_WORKSPACE]}--{this._httpContextAccessor.HttpContext.Request.Headers[TaxjarConstants.VTEX_ACCOUNT_HEADER_NAME]}.myvtex.com/taxjar/checkout/order-tax"
                };

                orderConfig["taxConfiguration"] = JToken.FromObject(taxConfiguration);

                jsonSerializedOrderConfig = JsonConvert.SerializeObject(orderConfig);
                bool success = await this._taxjarRepository.SetOrderConfiguration(jsonSerializedOrderConfig);
                retval = success.ToString();
            }

            return retval;
        }

        public async Task<TaxFallbackResponse> GetFallbackRate(string country, string postalCode, string provider = "avalara")
        {
            // GET https://vtexus.myvtex.com/_v/tax-fallback/{country}/{provider}/{postalCode}

            TaxFallbackResponse fallbackResponse = null;

            try
            {
                if (country.Length > 2)
                    country = country.Substring(0, 2);

                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri($"http://vtexus.myvtex.com/_v/tax-fallback/{country}/{provider}/{postalCode}")
                };

                request.Headers.Add(TaxjarConstants.USE_HTTPS_HEADER_NAME, "true");
                string authToken = this._httpContextAccessor.HttpContext.Request.Headers[TaxjarConstants.HEADER_VTEX_CREDENTIAL];
                if (authToken != null)
                {
                    request.Headers.Add(TaxjarConstants.AUTHORIZATION_HEADER_NAME, authToken);
                    request.Headers.Add(TaxjarConstants.VTEX_ID_HEADER_NAME, authToken);
                    request.Headers.Add(TaxjarConstants.PROXY_AUTHORIZATION_HEADER_NAME, authToken);
                }

                var client = _clientFactory.CreateClient();
                var response = await client.SendAsync(request);
                string responseContent = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    fallbackResponse = JsonConvert.DeserializeObject<TaxFallbackResponse>(responseContent);
                    Console.WriteLine($"GetFallbackRate [{response.StatusCode}] '{responseContent}'");
                }
                else
                {
                    _context.Vtex.Logger.Warn("GetFallbackRate", null, $"Did not get rates for {country} {postalCode} ({provider}) : '{response.Content}'");
                }
            }
            catch (Exception ex)
            {
                _context.Vtex.Logger.Error("GetFallbackRate", null, $"Error getting rates for {country} {postalCode} ({provider})", ex);
            }

            return fallbackResponse;
        }
    }
}
