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
            VtexTaxResponse vtexTaxResponse = null;
            Response.Headers.Add("Cache-Control", "private");
            if ("post".Equals(HttpContext.Request.Method, StringComparison.OrdinalIgnoreCase))
            {
                string bodyAsText = await new System.IO.StreamReader(HttpContext.Request.Body).ReadToEndAsync();
                if (!string.IsNullOrEmpty(bodyAsText))
                {
                    VtexTaxRequest taxRequest = JsonConvert.DeserializeObject<VtexTaxRequest>(bodyAsText);
                    if (taxRequest != null)
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

            Console.WriteLine($"TaxjarOrderTaxHandler Response = {JsonConvert.SerializeObject(vtexTaxResponse)}");

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
