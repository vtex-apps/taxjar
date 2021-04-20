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

    public class RoutesController : Controller
    {
        private readonly IIOServiceContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHttpClientFactory _clientFactory;
        private readonly ITaxjarService _taxjarService;
        private readonly ITaxjarRepository _taxjarRepository;
        private readonly IVtexAPIService _vtexAPIService;

        public RoutesController(IIOServiceContext context, IHttpContextAccessor httpContextAccessor, IHttpClientFactory clientFactory, ITaxjarService taxjarService, ITaxjarRepository taxjarRepository, IVtexAPIService vtexAPIService)
        {
            this._context = context ?? throw new ArgumentNullException(nameof(context));
            this._httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            this._clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
            this._taxjarService = taxjarService ?? throw new ArgumentNullException(nameof(taxjarService));
            this._taxjarRepository = taxjarRepository ?? throw new ArgumentNullException(nameof(taxjarRepository));
            this._vtexAPIService = vtexAPIService ?? throw new ArgumentNullException(nameof(vtexAPIService));
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
            Stopwatch timer = new Stopwatch();
            timer.Start();
            VtexTaxResponse vtexTaxResponse = new VtexTaxResponse
            {
                ItemTaxResponse = new ItemTaxResponse[0]
            };

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
                        List<string> dockIds = taxRequest.Items.Select(i => i.DockId).Distinct().ToList();
                        if (dockIds.Count > 1)
                        {
                            Console.WriteLine($" !!! SPLIT SHIPMENT ORDER !!! {dockIds.Count} LOCATIONS !!! ");
                            List<VtexTaxResponse> taxResponses = new List<VtexTaxResponse>();
                            decimal itemsTotal = taxRequest.Totals.Where(t => t.Id.Equals("Items")).Select(t => t.Value).FirstOrDefault();
                            decimal shippingTotal = taxRequest.Totals.Where(t => t.Id.Equals("Items")).Select(t => t.Value).FirstOrDefault();
                            long itemQuantity = taxRequest.Items.Sum(i => i.Quantity);
                            decimal itemsTotalSoFar = 0M;
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
                            foreach(VtexTaxResponse taxResponse in taxResponses)
                            {
                                foreach(ItemTaxResponse itemTaxResponse in taxResponse.ItemTaxResponse)
                                {
                                    itemTaxResponses.Add(itemTaxResponse);
                                }
                            }

                            vtexTaxResponse.ItemTaxResponse = itemTaxResponses.ToArray();
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
                                }
                                else
                                {
                                    Console.WriteLine("Null taxResponse");
                                }
                            }
                            else
                            {
                                Console.WriteLine("Null taxForOrder");
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
            
            if ("post".Equals(HttpContext.Request.Method, StringComparison.OrdinalIgnoreCase))
            {
                string bodyAsText = await new System.IO.StreamReader(HttpContext.Request.Body).ReadToEndAsync();
                InvoiceHookOrderStatus orderStatus = JsonConvert.DeserializeObject<InvoiceHookOrderStatus>(bodyAsText);
                if(orderStatus.Status.ToLower().Equals("invoiced"))
                {
                    VtexOrder vtexOrder = await _vtexAPIService.GetOrderInformation(orderStatus.OrderId);

                }
            }

            return Json("Order taxes were commited");
        }

        public async Task<IActionResult> InitConfig()
        {
            Response.Headers.Add("Cache-Control", "private");
            return Json(await _vtexAPIService.InitConfiguration());
        }
    }
}
