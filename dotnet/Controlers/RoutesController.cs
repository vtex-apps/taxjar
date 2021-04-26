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
            VtexTaxResponse vtexTaxResponse = new VtexTaxResponse
            {
                ItemTaxResponse = new ItemTaxResponse[0]
            };

            MerchantSettings merchantSettings = await _taxjarRepository.GetMerchantSettings();
            if(!merchantSettings.EnableTaxCalculations)
            {
                return Json(vtexTaxResponse);
            }

            bool useSummaryRates = false;
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
                    if (taxRequest != null)
                    {
                        decimal total = taxRequest.Totals.Sum(t => t.Value);
                        string cacheKey = $"{taxRequest.ShippingDestination.PostalCode}|{total}";
                        if (_memoryCache.TryGetValue(cacheKey, out vtexTaxResponse))
                        {
                            Console.WriteLine($"Fetched from Cache with key '{cacheKey}'.");
                            _context.Vtex.Logger.Info("TaxjarOrderTaxHandler", null, $"Taxes for '{cacheKey}' fetched from cache. {JsonConvert.SerializeObject(vtexTaxResponse)}");
                        }
                        else
                        {
                            bool inNexus = false;
                            VtexDockResponse[] vtexDocks = await _vtexAPIService.ListVtexDocks();
                            //List<string> nexusStates = vtexDocks.Select(d => d.PickupStoreInfo.Address.State).ToList();
                            List<string> nexusStates = new List<string>();
                            foreach (VtexDockResponse vtexDock in vtexDocks)
                            {
                                if (vtexDock.PickupStoreInfo != null && vtexDock.PickupStoreInfo.Address != null && vtexDock.PickupStoreInfo.Address.State != null)
                                {
                                    Console.WriteLine($"Nexus State '{vtexDock.PickupStoreInfo.Address.State}' Destination state '{taxRequest.ShippingDestination.State}' ");
                                    if (vtexDock.PickupStoreInfo.Address.State.Equals(taxRequest.ShippingDestination.State))
                                    {
                                        inNexus = true;
                                        Console.WriteLine($"Destination state '{taxRequest.ShippingDestination.State}' is in nexus");
                                        break;
                                    }
                                }
                            }

                            if (inNexus)
                            {
                                List<string> dockIds = taxRequest.Items.Select(i => i.DockId).Distinct().ToList();
                                if (dockIds.Count > 1)
                                {
                                    Console.WriteLine($" !!! SPLIT SHIPMENT ORDER !!! {dockIds.Count} LOCATIONS !!! ");
                                    List<VtexTaxResponse> taxResponses = new List<VtexTaxResponse>();
                                    decimal itemsTotal = taxRequest.Totals.Where(t => t.Id.Equals("Items")).Select(t => t.Value).FirstOrDefault();
                                    decimal shippingTotal = taxRequest.Totals.Where(t => t.Id.Equals("Items")).Select(t => t.Value).FirstOrDefault();
                                    long itemQuantity = taxRequest.Items.Sum(i => i.Quantity);
                                    //decimal itemsTotalSoFar = 0M;
                                    foreach (string dockId in dockIds)
                                    {
                                        List<Item> items = taxRequest.Items.Where(i => i.DockId.Equals(dockId)).ToList();
                                        long itemQuantityThisDock = items.Sum(i => i.Quantity);
                                        decimal percentOfWhole = itemQuantityThisDock / itemQuantity;
                                        VtexTaxRequest taxRequestThisDock = taxRequest;
                                        taxRequestThisDock.Items = items.ToArray();
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

                                        TaxForOrder taxForOrder = await _vtexAPIService.VtexRequestToTaxjarRequest(taxRequestThisDock);
                                        if (taxForOrder != null)
                                        {
                                            TaxResponse taxResponse = await _taxjarService.TaxForOrder(taxForOrder);
                                            if (taxResponse != null)
                                            {
                                                VtexTaxResponse vtexTaxResponseThisDock = await _vtexAPIService.TaxjarResponseToVtexResponse(taxResponse);
                                                taxResponses.Add(vtexTaxResponseThisDock);
                                            }
                                            else
                                            {
                                                useSummaryRates = true;
                                                Console.WriteLine("Null taxResponse");
                                            }
                                        }
                                        else
                                        {
                                            Console.WriteLine("Null taxForOrder");
                                        }
                                    }

                                    vtexTaxResponse.Hooks = taxResponses.FirstOrDefault().Hooks;
                                    List<ItemTaxResponse> itemTaxResponses = new List<ItemTaxResponse>();
                                    foreach (VtexTaxResponse taxResponse in taxResponses)
                                    {
                                        foreach (ItemTaxResponse itemTaxResponse in taxResponse.ItemTaxResponse)
                                        {
                                            itemTaxResponses.Add(itemTaxResponse);
                                        }
                                    }

                                    vtexTaxResponse.ItemTaxResponse = itemTaxResponses.ToArray();
                                    if (vtexTaxResponse != null)
                                    {
                                        var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
                                        _memoryCache.Set(cacheKey, vtexTaxResponse, cacheEntryOptions);
                                        Console.WriteLine($"Split Response saved to cache with key '{cacheKey}'");
                                        this.SummaryRates();
                                    }
                                }
                                else
                                {
                                    TaxForOrder taxForOrder = await _vtexAPIService.VtexRequestToTaxjarRequest(taxRequest);
                                    if (taxForOrder != null)
                                    {
                                        TaxResponse taxResponse = await _taxjarService.TaxForOrder(taxForOrder);
                                        if (taxResponse != null)
                                        {
                                            vtexTaxResponse = await _vtexAPIService.TaxjarResponseToVtexResponse(taxResponse);
                                            if (vtexTaxResponse != null)
                                            {
                                                var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
                                                _memoryCache.Set(cacheKey, vtexTaxResponse, cacheEntryOptions);
                                                Console.WriteLine($"Response saved to cache with key '{cacheKey}'");
                                                this.SummaryRates();
                                            }
                                        }
                                        else
                                        {
                                            useSummaryRates = true;
                                            Console.WriteLine("Null taxResponse");
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("Null taxForOrder");
                                    }
                                }

                                if (useSummaryRates)
                                {
                                    SummaryRatesStorage summaryRatesStorage = await _taxjarRepository.GetSummaryRates();
                                    if (summaryRatesStorage != null)
                                    {
                                        SummaryRatesResponse summaryRates = summaryRatesStorage.SummaryRatesResponse;
                                        List<SummaryRate> summaryRatesList = summaryRates.SummaryRates.Where(r => r.CountryCode.Equals(taxRequest.ShippingDestination.Country.Substring(0, 2))).ToList();
                                        summaryRatesList = summaryRatesList.Where(r => r.RegionCode.Equals(taxRequest.ShippingDestination.State)).ToList();
                                        decimal taxRate = summaryRatesList.Select(r => r.AverageRate.Rate).FirstOrDefault();
                                        //decimal orderTotal = taxRequest.Totals.Sum(t => t.Value);
                                        Console.WriteLine($"Using Summary Tax Rate of {taxRate} ");
                                        vtexTaxResponse = new VtexTaxResponse
                                        {
                                            Hooks = new Hook[]
                                            {
                                                new Hook
                                                {
                                                    Major = 1,
                                                    Url = new Uri($"https://{this._httpContextAccessor.HttpContext.Request.Headers[TaxjarConstants.HEADER_VTEX_WORKSPACE]}--{this._httpContextAccessor.HttpContext.Request.Headers[TaxjarConstants.VTEX_ACCOUNT_HEADER_NAME]}.myvtex.com/taxjar/oms/invoice")
                                                }
                                            },
                                            ItemTaxResponse = new ItemTaxResponse[taxRequest.Items.Length]
                                        };

                                        for (int i = 0; i < taxRequest.Items.Length; i++)
                                        {
                                            Item item = taxRequest.Items[i];
                                            ItemTaxResponse itemTaxResponse = new ItemTaxResponse
                                            {
                                                Id = item.Id
                                            };

                                            List<VtexTax> vtexTaxes = new List<VtexTax>();
                                            vtexTaxes.Add(
                                                new VtexTax
                                                {
                                                    Description = "",
                                                    Name = "ESTIMATED TAX",
                                                    Value = item.ItemPrice * taxRate
                                                }
                                             );

                                            Console.WriteLine($"[{item.Id}] #{item.Quantity} ${item.ItemPrice} * {taxRate}% = ${item.ItemPrice * taxRate}");
                                            itemTaxResponse.Taxes = vtexTaxes.ToArray();
                                            vtexTaxResponse.ItemTaxResponse[i] = itemTaxResponse;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Console.WriteLine($"Destination state '{taxRequest.ShippingDestination.State}' is NOT in nexus");
                                _context.Vtex.Logger.Info("TaxjarOrderTaxHandler", null, $"Order '{taxRequest.OrderFormId}' Destination state '{taxRequest.ShippingDestination.State}' is NOT in nexus");
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Null taxRequest");
                    }
                }
                else
                {
                    Console.WriteLine("Null body");
                }
            }

            //Console.WriteLine($"TaxjarOrderTaxHandler Response = {JsonConvert.SerializeObject(vtexTaxResponse)}");
            timer.Stop();
            //Console.WriteLine($"-> Elapsed Time = '{timer.Elapsed}' <-");

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
            Console.WriteLine($"{HttpContext.Request.Method} to ProcessInvoiceHook");
            if ("post".Equals(HttpContext.Request.Method, StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Reading request body...");
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
                                CreateTaxjarOrder taxjarOrder = await _vtexAPIService.VtexOrderToTaxjarOrder(vtexOrder);
                                _context.Vtex.Logger.Debug("CreateTaxjarOrder", null, $"{JsonConvert.SerializeObject(taxjarOrder)}");
                                OrderResponse orderResponse = await _taxjarService.CreateOrder(taxjarOrder);
                                if (orderResponse != null)
                                {
                                    _context.Vtex.Logger.Debug("ProcessInvoiceHook", null, $"Order '{orderStatus.OrderId}' taxes were commited");
                                    Console.WriteLine($"Order taxes were commited");
                                    return Ok("Order taxes were commited");
                                }
                                else
                                {
                                    Console.WriteLine($"Null OrderResponse!");
                                }
                            }
                        }
                        else
                        {
                            _context.Vtex.Logger.Debug("ProcessInvoiceHook", null, $"Transaction Posting is not enabled. Order '{orderStatus.OrderId}' ");
                            Console.WriteLine($"Transaction Posting is not enabled. Order '{orderStatus.OrderId}'");
                            return Ok("Transaction Posting is not enabled.");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Ignoring status '{orderStatus.Status}' for order '{orderStatus.OrderId}'");
                        return Ok("Ignoring status.");
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"Error processing invoice. {ex.Message}");
                    _context.Vtex.Logger.Error("ProcessInvoiceHook", null, "Error processing invoice.", ex);
                }
            }

            //return Json("Order taxes were commited");
            return BadRequest();
        }

        public async Task<IActionResult> InitConfig()
        {
            Response.Headers.Add("Cache-Control", "private");
            return Json(await _vtexAPIService.InitConfiguration());
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
                    Console.WriteLine("Loaded Rates from Storage");
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

                    Console.WriteLine("Updating Rates...");
                    _taxjarRepository.SetSummaryRates(summaryRatesStorage);
                }
            }

            return Json(summaryRatesResponse);
        }
    }
}