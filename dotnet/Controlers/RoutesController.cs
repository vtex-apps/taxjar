namespace Taxjar.Controllers
{
    using Taxjar.Data;
    using Taxjar.Models;
    using Taxjar.Services;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Vtex.Api.Context;
    using System.Web;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Diagnostics;
    using Microsoft.Extensions.Caching.Memory;

    public class RoutesController : Controller
    {
        private readonly IIOServiceContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHttpClientFactory _clientFactory;
        private readonly ITaxjarService _taxjarService;
        private readonly ITaxjarRepository _taxjarRepository;
        private readonly IVtexAPIService _vtexAPIService;
        private readonly IMemoryCache _memoryCache;

        public RoutesController(IIOServiceContext context, IHttpContextAccessor httpContextAccessor, IHttpClientFactory clientFactory, ITaxjarService taxjarService, ITaxjarRepository taxjarRepository, IVtexAPIService vtexAPIService, IMemoryCache memoryCache)
        {
            this._context = context ?? throw new ArgumentNullException(nameof(context));
            this._httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            this._clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
            this._taxjarService = taxjarService ?? throw new ArgumentNullException(nameof(taxjarService));
            this._taxjarRepository = taxjarRepository ?? throw new ArgumentNullException(nameof(taxjarRepository));
            this._vtexAPIService = vtexAPIService ?? throw new ArgumentNullException(nameof(vtexAPIService));
            this._memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        }

        public async Task<IActionResult> RatesForLocation(string zip)
        {
            Response.Headers.Add("Cache-Control", "private");
            var response = await _taxjarService.RatesForLocation(zip);
            return Json(response);
        }

        public async Task<IActionResult> Categories()
        {
            Response.Headers.Add("Cache-Control", "private");
            var response = await _taxjarService.Categories();
            return Json(response);
        }

        public async Task<IActionResult> TaxjarOrderTaxHandler()
        {
            bool fromCache = false;
            string orderFormId = string.Empty;
            long totalItems = 0;

            VtexTaxResponse vtexTaxResponse = new VtexTaxResponse
            {
                ItemTaxResponse = new ItemTaxResponse[0]
            };

            bool useFallbackRates = false;
            Stopwatch timer = new Stopwatch();
            timer.Start();

            Response.Headers.Add("Cache-Control", "private");
            Response.Headers.Add(TaxjarConstants.CONTENT_TYPE, TaxjarConstants.MINICART);
            if ("post".Equals(HttpContext.Request.Method, StringComparison.OrdinalIgnoreCase))
            {
                string bodyAsText = await new System.IO.StreamReader(HttpContext.Request.Body).ReadToEndAsync();
                if (!string.IsNullOrEmpty(bodyAsText))
                {
                    VtexTaxRequest taxRequest = JsonConvert.DeserializeObject<VtexTaxRequest>(bodyAsText);
                    VtexTaxRequest taxRequestOriginal = JsonConvert.DeserializeObject<VtexTaxRequest>(bodyAsText);
                    if (taxRequest != null && taxRequest.Items != null && taxRequest.Items.Length > 0)
                    {
                        _context.Vtex.Logger.Debug("TaxjarOrderTaxHandler", null, bodyAsText);
                        orderFormId = taxRequest.OrderFormId;
                        totalItems = taxRequest.Items.Sum(i => i.Quantity);
                        decimal total = taxRequest.Totals.Sum(t => t.Value);
                        if(total == 0)
                        {
                            timer.Stop();
                            _context.Vtex.Logger.Debug("TaxjarOrderTaxHandler", null, $"Elapsed Time = '{timer.Elapsed.TotalMilliseconds}' '{orderFormId}' {totalItems} items.  Order total is zero.");

                            return Json(vtexTaxResponse);
                        }

                        bool getDiscountFromOrderform = false;
                        try
                        {
                            decimal totalOrderDiscount = Math.Abs(taxRequest.Totals.Where(t => t.Id.Equals("Discounts")).Sum(t => t.Value));
                            decimal totalItemDiscount = Math.Abs(taxRequest.Items.Sum(i => i.DiscountPrice) * 100);
                            if (totalOrderDiscount != totalItemDiscount)
                            {
                                _context.Vtex.Logger.Warn("Discount", null, $"Order Discount {totalOrderDiscount} does not match Item Discount {totalItemDiscount} for order id '{taxRequest.OrderFormId}'");
                                getDiscountFromOrderform = true;
                            }
                        }
                        catch(Exception ex)
                        {
                            _context.Vtex.Logger.Error("Discount", null, $"Count not verify discounts for order id '{taxRequest.OrderFormId}'", ex);
                        }

                        // accountname+app+appversion+ 2021-04-23-4-20 + skuid+skuquantity+zipcode => turn this into a HASH
                        int cacheKey = $"{_context.Vtex.App.Version}{taxRequest.ShippingDestination.PostalCode}{totalItems}{total}".GetHashCode();
                        //if (_taxjarRepository.TryGetCache(cacheKey, out vtexTaxResponse))
                        //{
                        //    fromCache = true;
                        //    Console.WriteLine($"'{cacheKey}' fromCache = {fromCache} ");
                        //    _context.Vtex.Logger.Debug("TaxjarOrderTaxHandler", null, $"Taxes for '{cacheKey}' fetched from cache. {JsonConvert.SerializeObject(taxRequest)}\n\n{JsonConvert.SerializeObject(vtexTaxResponse)}");
                        //}
                        //else
                        {
                            bool inNexus = await this.InNexus(taxRequest.ShippingDestination.State, taxRequest.ShippingDestination.Country);

                            if (inNexus)
                            {
                                List<string> dockIds = new List<string>();
                                try
                                {
                                    dockIds = taxRequest.Items.Select(i => i.DockId).Distinct().ToList();
                                }
                                catch(Exception ex)
                                {
                                    _context.Vtex.Logger.Error("TaxjarOrderTaxHandler", null, $"Error getting dock ids {taxRequest.OrderFormId}", ex);
                                }

                                if (dockIds.Count > 1)
                                {
                                    try
                                    {
                                        List<VtexTaxResponse> taxResponses = new List<VtexTaxResponse>();
                                        decimal itemsTotal = taxRequest.Totals.Where(t => t.Id.Equals("Items")).Select(t => t.Value).FirstOrDefault();
                                        decimal shippingTotal = taxRequest.Totals.Where(t => t.Id.Equals("Items")).Select(t => t.Value).FirstOrDefault();
                                        long itemQuantity = taxRequest.Items.Sum(i => i.Quantity);
                                        foreach (string dockId in dockIds)
                                        {
                                            List<Item> items = taxRequest.Items.Where(i => i.DockId.Equals(dockId)).ToList();
                                            long itemQuantityThisDock = items.Sum(i => i.Quantity);
                                            decimal percentOfWhole = itemQuantityThisDock / itemQuantity;
                                            VtexTaxRequest taxRequestThisDock = new VtexTaxRequest
                                            {
                                                ClientData = new ClientData
                                                {
                                                    CorporateDocument = taxRequest.ClientData.CorporateDocument,
                                                    Document = taxRequest.ClientData.Document,
                                                    Email = taxRequest.ClientData.Email,
                                                    StateInscription = taxRequest.ClientData.StateInscription
                                                },
                                                ClientEmail = taxRequest.ClientEmail,
                                                Items = items.ToArray(),
                                                OrderFormId = taxRequest.OrderFormId,
                                                ShippingDestination = new ShippingDestination
                                                {
                                                    City = taxRequest.ShippingDestination.City,
                                                    Country = taxRequest.ShippingDestination.Country,
                                                    Neighborhood = taxRequest.ShippingDestination.Neighborhood,
                                                    PostalCode = taxRequest.ShippingDestination.PostalCode,
                                                    State = taxRequest.ShippingDestination.State,
                                                    Street = taxRequest.ShippingDestination.Street
                                                }
                                            };

                                            decimal itemsTotalThisDock = 0M;
                                            foreach (Item item in items)
                                            {
                                                itemsTotalThisDock += item.ItemPrice * item.Quantity;
                                            }

                                            taxRequestThisDock.Totals = new Total[]
                                            {
                                            new Total
                                            {
                                                Id = "Items",
                                                Name = "Items Total",
                                                Value = itemsTotalThisDock
                                            },
                                            new Total
                                            {
                                                Id = "Discounts",
                                                Name = "Discounts Total",
                                                Value = taxRequest.Totals.Where(t => t.Id.Equals("Discounts")).Select(t => t.Value).FirstOrDefault() * percentOfWhole
                                            },
                                            new Total
                                            {
                                                Id = "Shipping",
                                                Name = "Shipping Total",
                                                Value = taxRequest.Totals.Where(t => t.Id.Equals("Shipping")).Select(t => t.Value).FirstOrDefault() * percentOfWhole
                                            }
                                            };

                                            TaxForOrder taxForOrder = await _vtexAPIService.VtexRequestToTaxjarRequest(taxRequestThisDock, getDiscountFromOrderform);
                                            if (taxForOrder != null)
                                            {
                                                TaxResponse taxResponse = await _taxjarService.TaxForOrder(taxForOrder);
                                                if (taxResponse != null)
                                                {
                                                    VtexTaxResponse vtexTaxResponseThisDock = await _vtexAPIService.TaxjarResponseToVtexResponse(taxResponse, taxRequest, taxRequestOriginal);
                                                    _context.Vtex.Logger.Info("TaxjarOrderTaxHandler", null, $"Taxes for '{dockId}'\n{JsonConvert.SerializeObject(vtexTaxResponseThisDock)}");
                                                    if (vtexTaxResponseThisDock != null)
                                                    {
                                                        taxResponses.Add(vtexTaxResponseThisDock);
                                                    }
                                                }
                                                else
                                                {
                                                    useFallbackRates = true;
                                                }
                                            }
                                        }

                                        if (taxResponses != null && taxResponses.Count > 0)
                                        {
                                            if (taxResponses.Count == 1)
                                            {
                                                vtexTaxResponse = taxResponses.First();
                                            }
                                            else
                                            {
                                                try
                                                {
                                                    vtexTaxResponse.Hooks = taxResponses[0].Hooks;
                                                }
                                                catch (Exception ex)
                                                {
                                                    _context.Vtex.Logger.Error("TaxjarOrderTaxHandler", null, $"Order '{taxRequest.OrderFormId}' Error setting hooks", ex);
                                                }

                                                List<ItemTaxResponse> itemTaxResponses = new List<ItemTaxResponse>();
                                                foreach (var taxResponse in taxResponses)
                                                {
                                                    if (taxResponse != null && taxResponse.ItemTaxResponse != null)
                                                    {
                                                        foreach (var itemTaxResponse in taxResponse.ItemTaxResponse)
                                                        {
                                                            itemTaxResponses.Add(itemTaxResponse);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        _context.Vtex.Logger.Warn("TaxjarOrderTaxHandler", null, $"Order '{taxRequest.OrderFormId}' Null ItemTaxResponse\n{JsonConvert.SerializeObject(taxResponse)}");
                                                    }
                                                }

                                                try
                                                {
                                                    vtexTaxResponse = new VtexTaxResponse
                                                    {
                                                        ItemTaxResponse = itemTaxResponses.ToArray()
                                                    };
                                                }
                                                catch (Exception ex)
                                                {
                                                    _context.Vtex.Logger.Error("TaxjarOrderTaxHandler", null, $"Order '{taxRequest.OrderFormId}' Error setting ItemTaxResponse", ex);
                                                }

                                                if (vtexTaxResponse != null)
                                                {
                                                    await _taxjarRepository.SetCache(cacheKey, vtexTaxResponse);
                                                }
                                            }
                                        }
                                    }
                                    catch(Exception ex)
                                    {
                                        useFallbackRates = true;
                                        _context.Vtex.Logger.Error("TaxjarOrderTaxHandler", null, $"Error splitting by shipping origin {taxRequest.OrderFormId}", ex);
                                    }
                                }
                                else
                                {
                                    try
                                    {
                                        TaxForOrder taxForOrder = await _vtexAPIService.VtexRequestToTaxjarRequest(taxRequest, getDiscountFromOrderform);
                                        if (taxForOrder != null)
                                        {
                                            TaxResponse taxResponse = await _taxjarService.TaxForOrder(taxForOrder);
                                            if (taxResponse != null)
                                            {
                                                vtexTaxResponse = await _vtexAPIService.TaxjarResponseToVtexResponse(taxResponse,taxRequest, taxRequestOriginal);
                                                if (vtexTaxResponse != null)
                                                {
                                                    await _taxjarRepository.SetCache(cacheKey, vtexTaxResponse);
                                                }
                                            }
                                            else
                                            {
                                                useFallbackRates = true;
                                            }
                                        }
                                    }
                                    catch(Exception ex)
                                    {
                                        useFallbackRates = true;
                                        _context.Vtex.Logger.Error("TaxjarOrderTaxHandler", null, $"Error getting tax {taxRequest.OrderFormId}", ex);
                                    }
                                }

                                if (useFallbackRates)
                                {
                                    try
                                    {
                                        TaxFallbackResponse fallbackResponse = await _vtexAPIService.GetFallbackRate(taxRequest.ShippingDestination.Country, taxRequest.ShippingDestination.PostalCode);
                                        if (fallbackResponse != null)
                                        {
                                            vtexTaxResponse = new VtexTaxResponse
                                            {
                                                Hooks = new Hook[] { },
                                                ItemTaxResponse = new ItemTaxResponse[taxRequest.Items.Length]
                                            };

                                            long totalQuantity = taxRequest.Items.Sum(i => i.Quantity);
                                            for (int i = 0; i < taxRequest.Items.Length; i++)
                                            {
                                                Item item = taxRequest.Items[i];
                                                double itemTaxPercentOfWhole = (double)item.Quantity / totalQuantity;
                                                ItemTaxResponse itemTaxResponse = new ItemTaxResponse
                                                {
                                                    Id = item.Id
                                                };

                                                List<VtexTax> vtexTaxes = new List<VtexTax>();
                                                if (fallbackResponse.StateSalesTax > 0)
                                                {
                                                    vtexTaxes.Add(
                                                        new VtexTax
                                                        {
                                                            Description = "",
                                                            Name = $"STATE TAX: {fallbackResponse.StateAbbrev}",
                                                            Value = item.ItemPrice * fallbackResponse.StateSalesTax
                                                        }
                                                     );
                                                }

                                                if (fallbackResponse.CountySalesTax > 0)
                                                {
                                                    vtexTaxes.Add(
                                                    new VtexTax
                                                    {
                                                        Description = "",
                                                        Name = $"COUNTY TAX: {fallbackResponse.CountyName}",
                                                        Value = item.ItemPrice * fallbackResponse.CountySalesTax
                                                    }
                                                 );
                                                }

                                                if (fallbackResponse.CitySalesTax > 0)
                                                {
                                                    vtexTaxes.Add(
                                                    new VtexTax
                                                    {
                                                        Description = "",
                                                        Name = $"CITY TAX: {fallbackResponse.CityName}",
                                                        Value = item.ItemPrice * fallbackResponse.CitySalesTax
                                                    }
                                                 );
                                                }

                                                if (fallbackResponse.TaxShippingAlone || fallbackResponse.TaxShippingAndHandlingTogether)
                                                {
                                                    decimal shippingTotal = (decimal)taxRequest.Totals.Where(t => t.Id.Contains("Shipping")).Sum(t => t.Value) / 100;
                                                    vtexTaxes.Add(
                                                    new VtexTax
                                                    {
                                                        Description = "",
                                                        Name = $"TAX: (SHIPPING)",
                                                        Value = (decimal)Math.Round((double)shippingTotal * (double)fallbackResponse.TotalSalesTax * itemTaxPercentOfWhole, 2, MidpointRounding.ToEven)
                                                    }
                                                  );
                                                }

                                                itemTaxResponse.Taxes = vtexTaxes.ToArray();
                                                vtexTaxResponse.ItemTaxResponse[i] = itemTaxResponse;
                                            }
                                        }
                                    }
                                    catch(Exception ex)
                                    {
                                        _context.Vtex.Logger.Error("TaxjarOrderTaxHandler", null, $"Error getting fallback rates {taxRequest.OrderFormId}", ex);
                                    }
                                }
                            }
                            else
                            {
                                //_context.Vtex.Logger.Debug("TaxjarOrderTaxHandler", null, $"Order '{taxRequest.OrderFormId}' Destination state '{taxRequest.ShippingDestination.State}' is NOT in nexus");
                            }
                        }
                    }
                }
            }

            timer.Stop();
            _context.Vtex.Logger.Debug("TaxjarOrderTaxHandler", null, $"Elapsed Time = '{timer.Elapsed.TotalMilliseconds}' '{orderFormId}' {totalItems} items.  From cache? {fromCache}");
            //Console.WriteLine($"Elapsed Time = '{timer.Elapsed.TotalMilliseconds}' '{orderFormId}' {totalItems} items.  From cache? {fromCache}");
            //var taxes = vtexTaxResponse.ItemTaxResponse.Select(t => t.Taxes).ToList();
            //foreach(var taxValues in taxes)
            //{
            //    foreach (var taxValue in taxValues)
            //    {
            //        Console.WriteLine($"TAX = {taxValue.Name} = {taxValue.Value}");
            //    }
            //}
            
            return Json(vtexTaxResponse);
        }

        public async Task<IActionResult> TaxForOrder()
        {
            Response.Headers.Add("Cache-Control", "private");
            TaxResponse taxResponse = null;
            if ("post".Equals(HttpContext.Request.Method, StringComparison.OrdinalIgnoreCase))
            {
                string bodyAsText = await new System.IO.StreamReader(HttpContext.Request.Body).ReadToEndAsync();
                TaxForOrder taxForOrder = JsonConvert.DeserializeObject<TaxForOrder>(bodyAsText);
                taxResponse = await _taxjarService.TaxForOrder(taxForOrder);
            }

            return Json(taxResponse);
        }

        public async Task<IActionResult> ProcessInvoiceHook()
        {
            Response.Headers.Add("Cache-Control", "private");
            if ("post".Equals(HttpContext.Request.Method, StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    string bodyAsText = await new System.IO.StreamReader(HttpContext.Request.Body).ReadToEndAsync();
                    _context.Vtex.Logger.Debug("ProcessInvoiceHook", null, bodyAsText);
                    InvoiceHookOrderStatus orderStatus = JsonConvert.DeserializeObject<InvoiceHookOrderStatus>(bodyAsText);
                    if (orderStatus.Status.ToLower().Equals("invoiced"))
                    {
                        MerchantSettings merchantSettings = await _taxjarRepository.GetMerchantSettings();
                        if (merchantSettings.EnableTransactionPosting)
                        {
                            VtexOrder vtexOrder = await _vtexAPIService.GetOrderInformation(orderStatus.OrderId);
                            if (vtexOrder != null)
                            {
                                if (!string.IsNullOrEmpty(merchantSettings.SalesChannelExclude))
                                {
                                    string[] salesChannelsToExclude = merchantSettings.SalesChannelExclude.Split(',');
                                    if (salesChannelsToExclude.Contains(vtexOrder.SalesChannel))
                                    {
                                        return Ok($"Ignoring sales channel '{vtexOrder.SalesChannel}'.");
                                    }
                                }

                                CreateTaxjarOrder taxjarOrder = await _vtexAPIService.VtexOrderToTaxjarOrder(vtexOrder);
                                _context.Vtex.Logger.Debug("CreateTaxjarOrder", null, $"{JsonConvert.SerializeObject(taxjarOrder)}");
                                OrderResponse orderResponse = await _taxjarService.CreateOrder(taxjarOrder);
                                if (orderResponse != null)
                                {
                                    _context.Vtex.Logger.Debug("ProcessInvoiceHook", null, $"Order '{orderStatus.OrderId}' taxes were committed");
                                    return Ok("Order taxes were committed");
                                }
                            }
                        }
                        else
                        {
                            _context.Vtex.Logger.Debug("ProcessInvoiceHook", null, $"Transaction Posting is not enabled. Order '{orderStatus.OrderId}' ");
                            return Ok("Transaction Posting is not enabled.");
                        }
                    }
                    else
                    {
                        return Ok("Ignoring status.");
                    }
                }
                catch(Exception ex)
                {
                    _context.Vtex.Logger.Error("ProcessInvoiceHook", null, "Error processing invoice.", ex);
                }
            }

            //return Json("Order taxes were committed");
            return BadRequest();
        }

        public async Task<IActionResult> ProcessRefundHook()
        {
            Response.Headers.Add("Cache-Control", "private");
            if ("post".Equals(HttpContext.Request.Method, StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    string bodyAsText = await new System.IO.StreamReader(HttpContext.Request.Body).ReadToEndAsync();
                    _context.Vtex.Logger.Debug("ProcessRefundHook", null, bodyAsText);
                    RefundHook refundHook = JsonConvert.DeserializeObject<RefundHook>(bodyAsText);
                    MerchantSettings merchantSettings = await _taxjarRepository.GetMerchantSettings();
                    if (merchantSettings.EnableTransactionPosting)
                    {
                        VtexOrder vtexOrder = await _vtexAPIService.GetOrderInformation(refundHook.OrderId);
                        if (vtexOrder != null)
                        {
                            bool success = true;
                            List<Package> packages = vtexOrder.PackageAttachment.Packages.Where(p => p.Type.Equals("Input")).ToList();
                            foreach(Package package in packages)
                            {
                                CreateTaxjarOrder taxjarOrder = await _vtexAPIService.VtexPackageToTaxjarRefund(vtexOrder, package);
                                _context.Vtex.Logger.Debug("CreateTaxjarOrder", null, $"{JsonConvert.SerializeObject(taxjarOrder)}");
                                RefundResponse orderResponse = await _taxjarService.CreateRefund(taxjarOrder);
                                if (orderResponse != null)
                                {
                                    _context.Vtex.Logger.Debug("ProcessRefundHook", null, $"Order '{refundHook.OrderId}' refund was committed");
                                }
                                else
                                {
                                    success = false;
                                }
                            }

                            if(success)
                            {
                                return Ok("Refund was committed");
                            }
                        }
                    }
                    else
                    {
                        _context.Vtex.Logger.Debug("ProcessRefundHook", null, $"Transaction Posting is not enabled. Order '{refundHook.OrderId}' ");
                        return Ok("Transaction Posting is not enabled.");
                    }
                }
                catch(Exception ex)
                {
                    _context.Vtex.Logger.Error("ProcessRefundHook", null, "Error processing refund.", ex);
                    return BadRequest();
                }
            }

            return BadRequest();
        }

        public async Task<IActionResult> ValidateAddress()
        {
            Response.Headers.Add("Cache-Control", "private");
            if ("post".Equals(HttpContext.Request.Method, StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    string bodyAsText = await new System.IO.StreamReader(HttpContext.Request.Body).ReadToEndAsync();
                    _context.Vtex.Logger.Debug("ValidateAddress", null, bodyAsText);
                    Address validateAddress = JsonConvert.DeserializeObject<Address>(bodyAsText);
                    var response = await _taxjarService.ValidateAddress(validateAddress);
                    return Json(response);
                }
                catch(Exception ex)
                {
                    _context.Vtex.Logger.Error("ProcessRefundHook", null, "Error Validating Address.", ex);
                }
            }

            return BadRequest();
        }

        public async Task<IActionResult> InitConfig()
        {
            Response.Headers.Add("Cache-Control", "private");
            return Json(await _vtexAPIService.InitConfiguration());
        }

        public async Task<IActionResult> TaxjarToVtex()
        {
            VtexTaxResponse vtexTaxResponse = null;
            Response.Headers.Add("Cache-Control", "private");
            if ("post".Equals(HttpContext.Request.Method, StringComparison.OrdinalIgnoreCase))
            {
                string bodyAsText = await new System.IO.StreamReader(HttpContext.Request.Body).ReadToEndAsync();
                if (!string.IsNullOrEmpty(bodyAsText))
                {
                    TaxResponse taxResponse = JsonConvert.DeserializeObject<TaxResponse>(bodyAsText);
                    vtexTaxResponse = await _vtexAPIService.TaxjarResponseToVtexResponse(taxResponse, null, null);
                }
            }

            return Json(vtexTaxResponse);
        }

        public async Task<IActionResult> SummaryRates()
        {
            //Response.Headers.Add("Cache-Control", "private");
            SummaryRatesResponse summaryRatesResponse = null;
            SummaryRatesStorage summaryRatesStorage = await _taxjarRepository.GetSummaryRates();
            if(summaryRatesStorage != null)
            {
                TimeSpan ts = DateTime.Now - summaryRatesStorage.UpdatedAt;
                if(ts.TotalDays > 1)
                {
                    summaryRatesResponse = await _taxjarService.SummaryRates();
                    if(summaryRatesResponse != null)
                    {
                        summaryRatesStorage = new SummaryRatesStorage
                        {
                            UpdatedAt = DateTime.Now,
                            SummaryRatesResponse = summaryRatesResponse
                        };

                        _taxjarRepository.SetSummaryRates(summaryRatesStorage);
                    }
                }
                else
                {
                    summaryRatesResponse = summaryRatesStorage.SummaryRatesResponse;
                }
            }
            else
            {
                summaryRatesResponse = await _taxjarService.SummaryRates();
                if (summaryRatesResponse != null)
                {
                    summaryRatesStorage = new SummaryRatesStorage
                    {
                        UpdatedAt = DateTime.Now,
                        SummaryRatesResponse = summaryRatesResponse
                    };

                    _taxjarRepository.SetSummaryRates(summaryRatesStorage);
                }
            }

            return Json(summaryRatesResponse);
        }

        public async Task<bool> InNexus(string state, string country)
        {
            bool inNexus = false;
            try
            {
                country = _vtexAPIService.GetCountryCode(country);
                MerchantSettings merchantSettings = await _taxjarRepository.GetMerchantSettings();
                if (merchantSettings.UseTaxJarNexus)
                {
                    NexusRegionsResponse nexusRegionsResponse = await _vtexAPIService.NexusRegions();
                    if (nexusRegionsResponse != null && nexusRegionsResponse.Regions != null)
                    {
                        foreach (NexusRegion nexusRegion in nexusRegionsResponse.Regions)
                        {
                            if (nexusRegion != null && !string.IsNullOrEmpty(nexusRegion.CountryCode) && !string.IsNullOrEmpty(nexusRegion.RegionCode))
                            {
                                if (nexusRegion.RegionCode.Equals(state) && nexusRegion.CountryCode.Equals(country))
                                {
                                    inNexus = true;
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        //Console.WriteLine("Could not load nexus list from TaxJar.");
                        _context.Vtex.Logger.Warn("InNexus", null, "Could not load nexus list from TaxJar.");
                    }
                }
                else
                {
                    PickupPoints pickupPoints = await _vtexAPIService.ListPickupPoints();
                    List<string> nexusStates = new List<string>();
                    foreach (PickupPointItem pickupPoint in pickupPoints.Items)
                    {
                        if (pickupPoint != null && pickupPoint.Address != null && pickupPoint.Address.State != null && pickupPoint.Address.Country != null)
                        {
                            if (pickupPoint.Address.State.Equals(state) && _vtexAPIService.GetCountryCode(pickupPoint.Address.Country.Acronym).Equals(country))
                            {
                                inNexus = true;
                                break;
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                // if there is an error, try to collect tax
                inNexus = true;
                //Console.WriteLine($"ERROR: {ex.Message}");
            }
            
            return inNexus;
        }
    }
}