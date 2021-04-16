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
                VtexTaxRequest taxRequest = JsonConvert.DeserializeObject<VtexTaxRequest>(bodyAsText);
                TaxForOrder taxForOrder = await _vtexAPIService.VtexRequestToTaxjarRequest(taxRequest);
                TaxResponse taxResponse = await _taxjarService.TaxForOrder(taxForOrder);
            }

            return Json(vtexTaxResponse);
        }

        public async Task<IActionResult> TaxForOrder()
        {
            Response.Headers.Add("Cache-Control", "private");
            TaxForOrder taxForOrder = new TaxForOrder
            {
                Amount = 54.90F,
                NexusAddresses = new TaxForOrderNexusAddress[]
                {
                    new TaxForOrderNexusAddress
                    {
                        City = "Williamson",
                        Country = "US",
                        Id = "Main",
                        State = "NY",
                        Street = "1 Main",
                        Zip = "14589"
                    }
                },
                FromCity = "Williamson",
                FromCountry = "US",
                FromState = "NY",
                FromStreet = "1 Main",
                FromZip = "14589",
                LineItems = new TaxForOrderLineItem[]
                {
                    new TaxForOrderLineItem
                    {
                        Discount = 0F,
                        Id = "1",
                        ProductTaxCode = "20010",
                        Quantity = 2,
                        UnitPrice = 25.95F
                    }
                },
                Shipping = 3F,
                ToCity = "Rochester",
                ToCountry = "US",
                ToState = "NY",
                ToStreet = "10 East",
                ToZip = "14602"
            };

            var response = await _taxjarService.TaxForOrder(taxForOrder);
            return Json(response);
        }
    }
}
