namespace service.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using System;
    using Taxjar.Models;
    using Taxjar.Services;
    using Vtex.Api.Context;

    public class EventsController : Controller
    {
        private readonly IIOServiceContext _context;
        private readonly IVtexApiService _vtexAPIService;

        public EventsController(IIOServiceContext context, IVtexApiService vtexAPIService)
        {
            this._context = context ?? throw new ArgumentNullException(nameof(context));
            this._vtexAPIService = vtexAPIService ?? throw new ArgumentNullException(nameof(vtexAPIService));
        }

        public string OnAppsLinked(string account, string workspace)
        {
            return $"OnAppsLinked event detected for {account}/{workspace}";
        }

        public void AllStates(string account, string workspace)
        {
            string bodyAsText = new System.IO.StreamReader(HttpContext.Request.Body).ReadToEndAsync().Result;
            AllStatesNotification allStatesNotification = JsonConvert.DeserializeObject<AllStatesNotification>(bodyAsText);
            //_context.Vtex.Logger.Debug("Order Broadcast", null, $"Notification {bodyAsText}");
            bool success = _vtexAPIService.ProcessNotification(allStatesNotification).Result;
            if (!success)
            {
                _context.Vtex.Logger.Info("Order Broadcast", null, $"Failed to Process Notification {bodyAsText}");
                throw new Exception("Failed to Process Notification");
            }
        }
    }
}
