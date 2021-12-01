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
        private readonly ITaxjarService _taxjarService;
        private readonly string _applicationName;

        public VtexAPIService(IIOServiceContext context, IVtexEnvironmentVariableProvider environmentVariableProvider, IHttpContextAccessor httpContextAccessor, IHttpClientFactory clientFactory, ITaxjarRepository taxjarRepository, ITaxjarService taxjarService)
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

            this._taxjarService = taxjarService ??
                               throw new ArgumentNullException(nameof(taxjarService));

            this._applicationName =
                $"{this._environmentVariableProvider.ApplicationVendor}.{this._environmentVariableProvider.ApplicationName}";
        }

        public async Task<TaxForOrder> VtexRequestToTaxjarRequest(VtexTaxRequest vtexTaxRequest, bool getDiscountFromOrderform)
        {
            VtexDockResponse[] vtexDocks = await this.ListVtexDocks();
            PickupPoints pickupPoints = await this.ListPickupPoints();
            if(vtexDocks == null)
            {
                _context.Vtex.Logger.Error("VtexRequestToTaxjarRequest", null, "Could not load docks.");
                //return null;
            }

            string dockId = vtexTaxRequest.Items.Select(i => i.DockId).FirstOrDefault();
            //VtexDockResponse vtexDock = await this.ListDockById(dockId);
            VtexDockResponse vtexDock = vtexDocks.Where(d => d.Id.Equals(dockId)).FirstOrDefault();
            if(vtexDock == null)
            {
                _context.Vtex.Logger.Error("VtexRequestToTaxjarRequest", null, $"Dock '{dockId}' not found.");
            }

            string customerId = null;
            if(vtexTaxRequest.ClientData != null && vtexTaxRequest.ClientData.Email != null)
            {
                customerId = await this.GetShopperIdByEmail(vtexTaxRequest.ClientData.Email);
            }

            // Combine skus
            Dictionary<string, Item> itemDictionary = new Dictionary<string, Item>();
            foreach(Item requestItem in vtexTaxRequest.Items)
            {
                if(itemDictionary.ContainsKey(requestItem.Sku))
                {
                    itemDictionary[requestItem.Sku].DiscountPrice += requestItem.DiscountPrice;
                    itemDictionary[requestItem.Sku].ItemPrice += requestItem.ItemPrice;
                    itemDictionary[requestItem.Sku].Quantity += requestItem.Quantity;
                }
                else
                {
                    itemDictionary.Add(requestItem.Sku, requestItem);
                }
            }

            //vtexTaxRequest.Items = new Item[itemDictionary.Count];
            vtexTaxRequest.Items = itemDictionary.Values.ToArray();

            TaxForOrder taxForOrder = new TaxForOrder
            {
                //Amount = (float)vtexTaxRequest.Totals.Sum(t => t.Value) / 100,
                Shipping = (float)vtexTaxRequest.Totals.Where(t => t.Id.Equals("Shipping")).Sum(t => t.Value) / 100,
                //ToCity = vtexTaxRequest.ShippingDestination.City,
                //ToCountry = GetCountryCode(vtexTaxRequest.ShippingDestination.Country),
                //ToState = vtexTaxRequest.ShippingDestination.State,
                //ToStreet = vtexTaxRequest.ShippingDestination.Street,
                //ToZip = vtexTaxRequest.ShippingDestination.PostalCode,
                //FromCity = vtexDock.PickupStoreInfo.Address.City,
                //FromCountry = GetCountryCode(vtexDock.PickupStoreInfo.Address.Country.Acronym),
                //FromState = vtexDock.PickupStoreInfo.Address.State,
                //FromStreet = vtexDock.PickupStoreInfo.Address.Street,
                //FromZip = vtexDock.PickupStoreInfo.Address.PostalCode,
                CustomerId = customerId,
                LineItems = new TaxForOrderLineItem[vtexTaxRequest.Items.Length],
                //ExemptionType = TaxjarConstants.ExemptionType.NON_EXEMPT
                PlugIn = TaxjarConstants.PLUGIN
            };

            if(vtexTaxRequest.ShippingDestination != null)
            {
                taxForOrder.ToCity = vtexTaxRequest.ShippingDestination.City;
                taxForOrder.ToCountry = GetCountryCode(vtexTaxRequest.ShippingDestination.Country);
                taxForOrder.ToState = vtexTaxRequest.ShippingDestination.State;
                taxForOrder.ToStreet = vtexTaxRequest.ShippingDestination.Street;
                taxForOrder.ToZip = vtexTaxRequest.ShippingDestination.PostalCode;
            }
            else
            {
                _context.Vtex.Logger.Error("VtexRequestToTaxjarRequest", null, $"Missing Shipping Destination");
            }

            if(vtexDock != null && vtexDock.PickupStoreInfo != null && vtexDock.PickupStoreInfo.Address != null)
            {
                taxForOrder.FromCity = vtexDock.PickupStoreInfo.Address.City;
                taxForOrder.FromCountry = GetCountryCode(vtexDock.PickupStoreInfo.Address.Country.Acronym);
                taxForOrder.FromState = vtexDock.PickupStoreInfo.Address.State;
                taxForOrder.FromStreet = vtexDock.PickupStoreInfo.Address.Street;
                taxForOrder.FromZip = vtexDock.PickupStoreInfo.Address.PostalCode;
            }
            else
            {
                _context.Vtex.Logger.Error("VtexRequestToTaxjarRequest", null, $"Missing address for Dock {dockId}");
            }

            //Console.WriteLine($"Dock {dockId} # items = {vtexTaxRequest.Items.Length}");
            VtexOrderForm vtexOrderForm = null;
            if (getDiscountFromOrderform)
            {
                try
                {
                    vtexOrderForm = await GetOrderFormInformation(vtexTaxRequest.OrderFormId);
                }
                catch (Exception ex)
                {
                    //Console.WriteLine($"Error loading orderform {vtexTaxRequest.OrderFormId} {ex.Message}");
                    _context.Vtex.Logger.Error("VtexRequestToTaxjarRequest", null, $"Error loading orderform {vtexTaxRequest.OrderFormId}", ex);
                }
            }

            for (int i = 0; i < vtexTaxRequest.Items.Length; i++)
            {
                float discount = (float)Math.Abs(vtexTaxRequest.Items[i].DiscountPrice);
                if(getDiscountFromOrderform && vtexOrderForm != null)
                {
                    try
                    {
                        string sku = vtexTaxRequest.Items[i].Sku;
                        List<OrderformItem> orderformItems = vtexOrderForm.Items.Where(i => i.Id.Equals(sku)).ToList();
                        if (orderformItems != null && orderformItems.Count > 0)
                        {
                            float discountFromOrderform = 0f;
                            foreach (OrderformItem orderformItem in orderformItems)
                            {
                                long discountInCents = orderformItem.Price - orderformItem.SellingPrice;
                                discountInCents = discountInCents * orderformItem.Quantity;
                                discountFromOrderform += (float)discountInCents / 100f;
                                //Console.WriteLine($"Line [{i}] {sku} : {orderformItem.ListPrice} - {orderformItem.SellingPrice} * {orderformItem.Quantity} = {discountFromOrderform}");
                                _context.Vtex.Logger.Debug("VtexRequestToTaxjarRequest", "Discount", $"Line [{i}] {sku} : {orderformItem.ListPrice} - {orderformItem.SellingPrice} * {orderformItem.Quantity} = {discountFromOrderform}");
                            }

                            discountFromOrderform = (float)Math.Round(discountFromOrderform, 2, MidpointRounding.ToEven);

                            if (discount != discountFromOrderform)
                            {
                                //Console.WriteLine($"Line [{i}] Resetting discount for sku {sku} from {discount} to {discountFromOrderform}");
                                _context.Vtex.Logger.Warn("VtexRequestToTaxjarRequest", "Discount", $"Resetting discount for sku {sku} from {discount} to {discountFromOrderform} for order {vtexTaxRequest.OrderFormId}");
                                discount = discountFromOrderform;
                            }
                        }
                        else
                        {
                            //Console.WriteLine($"No match for sku {sku} in orderform {vtexTaxRequest.OrderFormId}");
                            _context.Vtex.Logger.Error("VtexRequestToTaxjarRequest", "Discount", $"No match for sku {sku} in orderform {vtexTaxRequest.OrderFormId}");
                        }
                    }
                    catch(Exception ex)
                    {
                        //Console.WriteLine($"Error getting discounts from orderform {vtexTaxRequest.OrderFormId}");
                        _context.Vtex.Logger.Error("VtexRequestToTaxjarRequest", "Discount", $"Error getting discounts from orderform {vtexTaxRequest.OrderFormId}", ex);
                    }
                }

                string taxCode = null;
                GetSkuContextResponse skuContextResponse = await this.GetSku(vtexTaxRequest.Items[i].Sku);
                if (skuContextResponse != null)
                {
                    taxCode = skuContextResponse.TaxCode;
                }
                else
                {
                    _context.Vtex.Logger.Warn($"GetSkuContextResponse was NULL for sku '{vtexTaxRequest.Items[i].Sku}' from order '{vtexTaxRequest.OrderFormId}'");
                }

                taxForOrder.LineItems[i] = new TaxForOrderLineItem
                {
                    Discount = discount,
                    Id = vtexTaxRequest.Items[i].Id,
                    ProductTaxCode = taxCode,
                    Quantity = vtexTaxRequest.Items[i].Quantity,
                    UnitPrice = (float)(vtexTaxRequest.Items[i].ItemPrice / vtexTaxRequest.Items[i].Quantity)
                };

                //Console.WriteLine($"[{taxForOrder.LineItems[i].Id}] x{taxForOrder.LineItems[i].Quantity} {taxForOrder.LineItems[i].UnitPrice} - {taxForOrder.LineItems[i].Discount}");
            }

            List<TaxForOrderNexusAddress> nexuses = new List<TaxForOrderNexusAddress>();
            MerchantSettings merchantSettings = await _taxjarRepository.GetMerchantSettings();
            if(merchantSettings.UseTaxJarNexus)
            {
                _context.Vtex.Logger.Debug("VtexRequestToTaxjarRequest", null, "Using TaxJar for nexus.");
            }
            else
            {
                foreach(PickupPointItem pickupPoint in pickupPoints.Items)
                {
                    if (pickupPoint.TagsLabel.Any(t => t.Contains(TaxjarConstants.PICKUP_TAG, StringComparison.OrdinalIgnoreCase)))
                    {
                        if(pickupPoint.Address != null)
                        {
                            nexuses.Add(
                                    new TaxForOrderNexusAddress
                                    {
                                        City = pickupPoint.Address.City,
                                        Country = string.IsNullOrEmpty(pickupPoint.Address.Country.Acronym) ? string.Empty : GetCountryCode(pickupPoint.Address.Country.Acronym),
                                        Id = pickupPoint.PickupPointId,
                                        State = pickupPoint.Address.State,
                                        Street = pickupPoint.Address.Street,
                                        Zip = pickupPoint.Address.PostalCode
                                    }
                                );
                        }
                        else
                        {
                            _context.Vtex.Logger.Warn("VtexRequestToTaxjarRequest", null, $"PickupPoint {pickupPoint.PickupPointId} missing address");
                        }
                    }
                }
            }

            taxForOrder.NexusAddresses = nexuses.ToArray();

            _context.Vtex.Logger.Info("VtexRequestToTaxjarRequest", null, $"Request: {JsonConvert.SerializeObject(vtexTaxRequest)}\nResponse: {JsonConvert.SerializeObject(taxForOrder)}");

            return taxForOrder;
        }

        public async Task<VtexTaxResponse> TaxjarResponseToVtexResponse(TaxResponse taxResponse, VtexTaxRequest taxRequest, VtexTaxRequest taxRequestOriginal)
        {
            if(taxResponse == null)
            {
                return null;
            }

            if (taxResponse.Tax == null)
            {
                return null;
            }

            if (taxResponse.Tax.Breakdown == null)
            {
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

            double shippingTaxCity = 0d;
            double shippingTaxCounty = 0d;
            double shippingTaxSpecial = 0d;
            double shippingTaxState = 0d;
            double shippingTaxGST = 0d;
            double shippingTaxPST = 0d;
            double shippingTaxQST = 0d;

            if(taxResponse.Tax.Breakdown.Shipping != null)
            {
                shippingTaxCity = (double)taxResponse.Tax.Breakdown.Shipping.CityAmount;
                shippingTaxCounty = (double)taxResponse.Tax.Breakdown.Shipping.CountyAmount;
                shippingTaxSpecial = (double)taxResponse.Tax.Breakdown.Shipping.SpecialDistrictAmount;
                shippingTaxState = (double)taxResponse.Tax.Breakdown.Shipping.StateAmount;
                shippingTaxGST = (double)taxResponse.Tax.Breakdown.Shipping.GST;
                shippingTaxPST = (double)taxResponse.Tax.Breakdown.Shipping.PST;
                shippingTaxQST = (double)taxResponse.Tax.Breakdown.Shipping.QST;
            }

            double totalItemTax = (double)taxResponse.Tax.Breakdown.LineItems.Sum(i => i.TaxCollectable);

            for (int i = 0; i < taxResponse.Tax.Breakdown.LineItems.Count; i++)
            {
                TaxBreakdownLineItem lineItem = taxResponse.Tax.Breakdown.LineItems[i];
                double itemTaxPercentOfWhole = 0;
                if (totalItemTax > 0)
                {
                    itemTaxPercentOfWhole = (double)lineItem.TaxCollectable / totalItemTax;
                }
                else
                {
                    itemTaxPercentOfWhole = 1 / (double)taxResponse.Tax.Breakdown.LineItems.Count;
                }

                ItemTaxResponse itemTaxResponse = new ItemTaxResponse
                {
                    Id = lineItem.Id
                };

                List<VtexTax> vtexTaxes = new List<VtexTax>();
                //if (lineItem.StateAmount > 0)
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
                    vtexTaxes.Add(
                        new VtexTax
                        {
                            Description = "",
                            Name = "SPECIAL TAX (SHIPPING)",
                            Value = (decimal)Math.Round(shippingTaxSpecial * itemTaxPercentOfWhole, 2, MidpointRounding.ToEven)
                        }
                     );
                }

                if (lineItem.GST > 0)
                {
                    vtexTaxes.Add(
                        new VtexTax
                        {
                            Description = "",
                            Name = $"GST",
                            Value = lineItem.GST
                        }
                     );
                }

                if (lineItem.PST > 0)
                {
                    vtexTaxes.Add(
                        new VtexTax
                        {
                            Description = "",
                            Name = $"PST",
                            Value = lineItem.PST
                        }
                     );
                }

                if (lineItem.QST > 0)
                {
                    vtexTaxes.Add(
                        new VtexTax
                        {
                            Description = "",
                            Name = $"QST",
                            Value = lineItem.QST
                        }
                     );
                }

                if (shippingTaxGST > 0)
                {
                    vtexTaxes.Add(
                        new VtexTax
                        {
                            Description = "",
                            Name = $"GST (SHIPPING)",
                            Value = (decimal)Math.Round(shippingTaxGST * itemTaxPercentOfWhole, 2, MidpointRounding.ToEven)
                        }
                     );
                }

                if (shippingTaxPST > 0)
                {
                    vtexTaxes.Add(
                        new VtexTax
                        {
                            Description = "",
                            Name = $"PST (SHIPPING)",
                            Value = (decimal)Math.Round(shippingTaxPST * itemTaxPercentOfWhole, 2, MidpointRounding.ToEven)
                        }
                     );
                }

                if (shippingTaxQST > 0)
                {
                    vtexTaxes.Add(
                        new VtexTax
                        {
                            Description = "",
                            Name = $"QST (SHIPPING)",
                            Value = (decimal)Math.Round(shippingTaxQST * itemTaxPercentOfWhole, 2, MidpointRounding.ToEven)
                        }
                     );
                }

                itemTaxResponse.Taxes = vtexTaxes.ToArray();
                vtexTaxResponse.ItemTaxResponse[i] = itemTaxResponse;
            };

            try
            {
                // Split out skus to match request
                if (taxResponse.Tax.Breakdown.LineItems.Count != taxRequestOriginal.Items.Length)
                {
                    ItemTaxResponse[] itemTaxResponses = new ItemTaxResponse[taxRequestOriginal.Items.Length];
                    int responseId = 0;
                    int taxResponseIndex = 0;
                    decimal totalSplitAllocatedTax = 0m;
                    Dictionary<string, long> qntyAllocatedPerSku = new Dictionary<string, long>();
                    foreach (Item requestItem in taxRequestOriginal.Items)
                    {
                        Item trItem = taxRequest.Items.Where(i => i.Sku.Equals(requestItem.Sku)).FirstOrDefault();
                        if (requestItem.Quantity == trItem.Quantity)
                        {
                            itemTaxResponses[responseId] = vtexTaxResponse.ItemTaxResponse[taxResponseIndex];
                            taxResponseIndex++;
                        }
                        else
                        {
                            decimal percentOfTotal = 0m;
                            if (trItem.ItemPrice + trItem.DiscountPrice > 0)
                            {
                                percentOfTotal = (decimal)(requestItem.ItemPrice + requestItem.DiscountPrice) / (decimal)(trItem.ItemPrice + trItem.DiscountPrice);
                            }

                            Console.WriteLine($"[{responseId}] {(decimal)(requestItem.ItemPrice + requestItem.DiscountPrice)} / {(decimal)(trItem.ItemPrice + trItem.DiscountPrice)} = {percentOfTotal}");

                            //decimal percentOfTotal = (decimal)requestItem.Quantity / (decimal)trItem.Quantity;
                            ItemTaxResponse itemTaxResponse = vtexTaxResponse.ItemTaxResponse[taxResponseIndex];
                            ItemTaxResponse itemTaxResponseSplit = new ItemTaxResponse
                            {
                                Id = responseId.ToString(),
                                Taxes = new VtexTax[itemTaxResponse.Taxes.Length]
                            };

                            long taxObjIndex = 0;
                            foreach (VtexTax taxObj in itemTaxResponse.Taxes)
                            {
                                itemTaxResponseSplit.Taxes[taxObjIndex] = new VtexTax
                                {
                                    Description = taxObj.Description,
                                    Name = taxObj.Name,
                                    Value = Math.Round(taxObj.Value * percentOfTotal, 2, MidpointRounding.ToEven)
                                };

                                Console.WriteLine($"[{taxResponseIndex}][{taxObjIndex}] {taxObj.Value} * {percentOfTotal} = {Math.Round(taxObj.Value * percentOfTotal, 2, MidpointRounding.ToEven)} {taxObj.Name} ");
                                taxObjIndex++;
                            }

                            itemTaxResponses[responseId] = itemTaxResponseSplit;
                            totalSplitAllocatedTax += itemTaxResponseSplit.Taxes.Sum(i => i.Value);

                            if (qntyAllocatedPerSku.ContainsKey(requestItem.Sku))
                            {
                                qntyAllocatedPerSku[requestItem.Sku] += requestItem.Quantity;
                            }
                            else
                            {
                                qntyAllocatedPerSku.Add(requestItem.Sku, requestItem.Quantity);
                            }

                            if(qntyAllocatedPerSku[requestItem.Sku] == trItem.Quantity)
                            {
                                decimal totalTaxToAllocate = itemTaxResponse.Taxes.Sum(i => i.Value);
                                if (totalSplitAllocatedTax != totalTaxToAllocate)
                                {
                                    decimal adjustmentAmount = Math.Round((totalTaxToAllocate - totalSplitAllocatedTax), 2, MidpointRounding.ToEven);
                                    itemTaxResponses[responseId].Taxes.First().Value += adjustmentAmount;
                                    _context.Vtex.Logger.Warn("TaxjarResponseToVtexResponse", null, $"Applying adjustment to id [{taxResponseIndex}]: {totalTaxToAllocate} - {totalSplitAllocatedTax} = {adjustmentAmount}");
                                }

                                taxResponseIndex++;
                                totalSplitAllocatedTax = 0m;
                            }
                        }

                        responseId++;
                    }

                    vtexTaxResponse.ItemTaxResponse = itemTaxResponses;
                }
            }
            catch(Exception ex)
            {
                _context.Vtex.Logger.Error("TaxjarResponseToVtexResponse", "Splitting", $"Error splitting line items", ex);
            }

            decimal totalOrderTax = (decimal)taxResponse.Tax.AmountToCollect;
            decimal totalResponseTax = vtexTaxResponse.ItemTaxResponse.SelectMany(t => t.Taxes).Sum(i => i.Value);
            if(!totalOrderTax.Equals(totalResponseTax))
            {
                try
                {
                    decimal adjustmentAmount = Math.Round((totalOrderTax - totalResponseTax), 2, MidpointRounding.ToEven);
                    _context.Vtex.Logger.Warn("TaxjarResponseToVtexResponse", null, $"Applying adjustment: {totalOrderTax} - {totalResponseTax} = {adjustmentAmount}");
                    //Console.WriteLine($"Applying adjustment: {totalOrderTax} - {totalResponseTax} = {adjustmentAmount}");
                    for (int lastItemIndex = vtexTaxResponse.ItemTaxResponse.Length - 1; lastItemIndex >= 0; lastItemIndex--)
                    {
                        if (vtexTaxResponse.ItemTaxResponse[lastItemIndex] != null && vtexTaxResponse.ItemTaxResponse[lastItemIndex].Taxes != null)
                        {
                            for (int lastTaxIndex = vtexTaxResponse.ItemTaxResponse[lastItemIndex].Taxes.Length - 1; lastTaxIndex >= 0; lastTaxIndex--)
                            {
                                if (vtexTaxResponse.ItemTaxResponse[lastItemIndex].Taxes[lastTaxIndex].Value > 0 && vtexTaxResponse.ItemTaxResponse[lastItemIndex].Taxes[lastTaxIndex].Value + adjustmentAmount >= 0)
                                {
                                    vtexTaxResponse.ItemTaxResponse[lastItemIndex].Taxes[lastTaxIndex].Value += adjustmentAmount;
                                    adjustmentAmount = 0;
                                    break;
                                }
                            }
                        }

                        if(adjustmentAmount == 0)
                        {
                            break;
                        }
                    }
                }
                catch(Exception ex)
                {
                    _context.Vtex.Logger.Error("TaxjarResponseToVtexResponse", null, $"Error applying adjustment", ex);
                }
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
                ToCountry = GetCountryCode(vtexOrder.ShippingData.Address.Country),
                ToZip = vtexOrder.ShippingData.Address.PostalCode,
                ToState = vtexOrder.ShippingData.Address.State,
                ToCity = vtexOrder.ShippingData.Address.City,
                ToStreet = vtexOrder.ShippingData.Address.Street,
                Amount = (decimal)vtexOrder.Totals.Where(t => !t.Id.Contains("Tax")).Sum(t => t.Value) / 100,
                Shipping = (decimal)vtexOrder.Totals.Where(t => t.Id.Contains("Shipping")).Sum(t => t.Value) / 100,
                SalesTax = (decimal)vtexOrder.Totals.Where(t => t.Id.Contains("Tax")).Sum(t => t.Value) / 100,
                CustomerId = await this.GetShopperIdByEmail(vtexOrder.ClientProfileData.Email),
                //ExemptionType = TaxjarConstants.ExemptionType.NON_EXEMPT,
                LineItems = new List<LineItem>(),
                PlugIn = TaxjarConstants.PLUGIN
            };

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
                        taxjarOrder.FromCountry = GetCountryCode(vtexDock.PickupStoreInfo.Address.Country.Acronym);
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
                //Console.WriteLine($"[{taxForOrderLineItem.Id}] x{taxForOrderLineItem.Quantity} {taxForOrderLineItem.UnitPrice} - {taxForOrderLineItem.Discount}");
            }

            return taxjarOrder;
        }

        public async Task<CreateTaxjarOrder> VtexPackageToTaxjarRefund(VtexOrder vtexOrder, Package package)
        {
            //var refunds = vtexOrder.PackageAttachment.Packages.Where(p => p.Type.Equals("Input"));

            long totalRefundAmout = package.Restitutions.Refund.Value;
            //long totalItemAmount = package.Restitutions.Refund.Items.Sum(r => r.Price * r.Quantity);
            long totalItemAmount = 0;
            long totalTaxAmount = 0;
            long totalShippingAmount = 0;
            List<LineItem> taxJarItems = new List<LineItem>();
            foreach(RefundItem refundItem in package.Restitutions.Refund.Items)
            {
                VtexOrderItem orderItem = vtexOrder.Items.Where(i => i.Id.Equals(refundItem.Id)).FirstOrDefault();
                totalItemAmount += refundItem.Price * refundItem.Quantity;
                long taxForReturnedItems = (orderItem.PriceTags.Where(t => t.Name.Contains("TAX")).Sum(t => t.Value) / orderItem.Quantity) * refundItem.Quantity;
                totalTaxAmount += taxForReturnedItems;
                
                GetSkuContextResponse contextResponse = await this.GetSku(orderItem.SellerSku);
                LineItem taxForOrderLineItem = new LineItem
                {
                    Id = refundItem.Id,
                    Quantity = (int)refundItem.Quantity,
                    ProductIdentifier = orderItem.SellerSku,
                    Description = orderItem.Name,
                    ProductTaxCode = contextResponse.TaxCode,
                    UnitPrice = (decimal)refundItem.Price * -1 / 100,
                    //Discount = Math.Abs((decimal)orderItem.PriceTags.Where(p => p.Name.Contains("DISCOUNT@")).Sum(p => p.Value) / 100),
                    SalesTax = (decimal)taxForReturnedItems * -1 / 100
                };

                taxJarItems.Add(taxForOrderLineItem);
            }

            totalShippingAmount = totalRefundAmout - totalItemAmount;

            CreateTaxjarOrder taxjarOrder = new CreateTaxjarOrder
            {
                TransactionId = package.InvoiceNumber,
                TransactionReferenceId = vtexOrder.OrderId,
                TransactionDate = DateTime.Parse(package.IssuanceDate).ToString("yyyy-MM-ddTHH:mm:sszzz"),
                Provider = vtexOrder.SalesChannel,
                ToCountry = GetCountryCode(vtexOrder.ShippingData.Address.Country),
                ToZip = vtexOrder.ShippingData.Address.PostalCode,
                ToState = vtexOrder.ShippingData.Address.State,
                ToCity = vtexOrder.ShippingData.Address.City,
                ToStreet = vtexOrder.ShippingData.Address.Street,
                Amount = (decimal)(totalItemAmount + totalShippingAmount) * -1 / 100, // Total amount of the refunded order with shipping, excluding sales tax in dollars.
                Shipping = (decimal)totalShippingAmount * -1 / 100, // Total amount of shipping for the refunded order in dollars.
                SalesTax = (decimal)totalTaxAmount * -1 / 100, // Total amount of sales tax collected for the refunded order in dollars.
                CustomerId = await this.GetShopperIdByEmail(vtexOrder.ClientProfileData.Email),
                //ExemptionType = TaxjarConstants.ExemptionType.NON_EXEMPT,
                LineItems = taxJarItems,
                PlugIn = TaxjarConstants.PLUGIN
            };

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
                        taxjarOrder.FromCountry = GetCountryCode(vtexDock.PickupStoreInfo.Address.Country.Acronym);
                        taxjarOrder.FromZip = vtexDock.PickupStoreInfo.Address.PostalCode;
                        taxjarOrder.FromState = vtexDock.PickupStoreInfo.Address.State;
                        taxjarOrder.FromCity = vtexDock.PickupStoreInfo.Address.City;
                        taxjarOrder.FromStreet = vtexDock.PickupStoreInfo.Address.Street;
                    }
                }
            }

            return taxjarOrder;
        }

        public async Task<VtexOrder> GetOrderInformation(string orderId)
        {
            VtexOrder vtexOrder = null;

            try
            {
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri($"http://{this._httpContextAccessor.HttpContext.Request.Headers[TaxjarConstants.VTEX_ACCOUNT_HEADER_NAME]}.{TaxjarConstants.ENVIRONMENT}.com.br/api/oms/pvt/orders/{orderId}")
                };

                request.Headers.Add(TaxjarConstants.USE_HTTPS_HEADER_NAME, "true");
                string authToken = this._httpContextAccessor.HttpContext.Request.Headers[TaxjarConstants.HEADER_VTEX_CREDENTIAL];
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
                }
                else
                {
                    _context.Vtex.Logger.Info("GetOrderInformation", null, $"Order# {orderId} [{response.StatusCode}] '{responseContent}'");
                }
            }
            catch (Exception ex)
            {
                _context.Vtex.Logger.Error("GetOrderInformation", null, $"Order# {orderId} Error", ex);
            }

            return vtexOrder;
        }

        public async Task<VtexOrderForm> GetOrderFormInformation(string orderId)
        {
            VtexOrderForm vtexOrder = null;

            try
            {
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri($"http://{this._httpContextAccessor.HttpContext.Request.Headers[TaxjarConstants.VTEX_ACCOUNT_HEADER_NAME]}.{TaxjarConstants.ENVIRONMENT}.com.br/api/checkout/pub/orderForm/{orderId}?disableAutoCompletion=true")
                };

                request.Headers.Add(TaxjarConstants.USE_HTTPS_HEADER_NAME, "true");
                string authToken = this._httpContextAccessor.HttpContext.Request.Headers[TaxjarConstants.HEADER_VTEX_CREDENTIAL];
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
                    vtexOrder = JsonConvert.DeserializeObject<VtexOrderForm>(responseContent);
                }
                else
                {
                    _context.Vtex.Logger.Info("GetOrderInformation", null, $"Order# {orderId} [{response.StatusCode}] '{responseContent}'");
                }
            }
            catch (Exception ex)
            {
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

            var client = _clientFactory.CreateClient();
            var response = await client.SendAsync(request);
            string responseContent = await response.Content.ReadAsStringAsync();
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

            var client = _clientFactory.CreateClient();
            var response = await client.SendAsync(request);
            string responseContent = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                dockResponse = JsonConvert.DeserializeObject<VtexDockResponse>(responseContent);
            }

            return dockResponse;
        }

        public async Task<PickupPoints> ListPickupPoints()
        {
            PickupPoints pickupPoints = null;
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"http://logistics.vtexcommercestable.com.br/api/logistics/pvt/configuration/pickuppoints/_search?an={this._httpContextAccessor.HttpContext.Request.Headers[TaxjarConstants.VTEX_ACCOUNT_HEADER_NAME]}&pageSize=100")
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
            if (response.IsSuccessStatusCode)
            {
                pickupPoints = JsonConvert.DeserializeObject<PickupPoints>(responseContent);
            }

            return pickupPoints;
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
            MerchantSettings merchantSettings = await _taxjarRepository.GetMerchantSettings();
            string jsonSerializedOrderConfig = await this._taxjarRepository.GetOrderConfiguration();
            if (string.IsNullOrEmpty(jsonSerializedOrderConfig))
            {
                //Console.WriteLine("------------ Could not load Order Configuration. ----------------------");
                retval = "Could not load Order Configuration.";
            }
            else
            {
                dynamic orderConfig = JsonConvert.DeserializeObject(jsonSerializedOrderConfig);
                VtexOrderformTaxConfiguration taxConfiguration = new VtexOrderformTaxConfiguration
                {
                    AllowExecutionAfterErrors = false,
                    IntegratedAuthentication = true,
                    Url = $"https://{this._httpContextAccessor.HttpContext.Request.Headers[TaxjarConstants.HEADER_VTEX_WORKSPACE]}--{this._httpContextAccessor.HttpContext.Request.Headers[TaxjarConstants.VTEX_ACCOUNT_HEADER_NAME]}.myvtex.com/taxjar/checkout/order-tax"
                };

                orderConfig["taxConfiguration"] = JToken.FromObject(taxConfiguration);

                jsonSerializedOrderConfig = JsonConvert.SerializeObject(orderConfig);
                bool success = await this._taxjarRepository.SetOrderConfiguration(jsonSerializedOrderConfig);
                retval = success.ToString();
            }

            return retval;
        }

        public async Task<string> RemoveConfiguration()
        {
            string retval = string.Empty;
            MerchantSettings merchantSettings = await _taxjarRepository.GetMerchantSettings();
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
                country = GetCountryCode(country);

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

        public async Task<bool> ProcessNotification(AllStatesNotification allStatesNotification)
        {
            bool success = true;
            VtexOrder vtexOrder = null;

            switch (allStatesNotification.Domain)
            {
                case TaxjarConstants.Domain.Fulfillment:
                    switch (allStatesNotification.CurrentState)
                    {
                        case TaxjarConstants.VtexOrderStatus.Invoiced:
                            success = await this.ProcessInvoice(allStatesNotification.OrderId);
                            break;
                        break;
                        default:
                            //_context.Vtex.Logger.Info("ProcessNotification", null, $"State {hookNotification.State} not implemeted.");
                            break;
                    }
                    break;
                case TaxjarConstants.Domain.Marketplace:
                    switch (allStatesNotification.CurrentState)
                    {
                        default:
                            //_context.Vtex.Logger.Info("ProcessNotification", null, $"State {hookNotification.State} not implemeted.");
                            break;
                    }
                    break;
                default:
                    //_context.Vtex.Logger.Info("ProcessNotification", null, $"Domain {hookNotification.Domain} not implemeted.");
                    break;
            }

            return success;
        }

        public async Task<bool> ProcessInvoice(string orderId)
        {
            bool success = true;
            MerchantSettings merchantSettings = await _taxjarRepository.GetMerchantSettings();
            if (merchantSettings.EnableTransactionPosting)
            {
                VtexOrder vtexOrder = await this.GetOrderInformation(orderId);
                if (vtexOrder != null)
                {
                    if (!string.IsNullOrEmpty(merchantSettings.SalesChannelExclude))
                    {
                        string[] salesChannelsToExclude = merchantSettings.SalesChannelExclude.Split(',');
                        if (salesChannelsToExclude.Contains(vtexOrder.SalesChannel))
                        {
                            _context.Vtex.Logger.Debug("ProcessInvoiceHook", null, $"Order '{orderId}' skipping sales channel '{vtexOrder.SalesChannel}'");
                            return success;
                        }
                    }

                    CreateTaxjarOrder taxjarOrder = await this.VtexOrderToTaxjarOrder(vtexOrder);
                    _context.Vtex.Logger.Debug("CreateTaxjarOrder", null, $"{JsonConvert.SerializeObject(taxjarOrder)}");
                    OrderResponse orderResponse = await _taxjarService.CreateOrder(taxjarOrder);
                    if (orderResponse != null)
                    {
                        _context.Vtex.Logger.Debug("ProcessInvoiceHook", null, $"Order '{orderId}' taxes were committed");
                    }
                    else
                    {
                        success = false;
                    }
                }
            }

            return success;
        }

        public async Task<VtexUser[]> GetShopperByEmail(string email)
        {
            // GET https://{accountName}.{environment}.com.br/api/dataentities/CL/search?email=

            VtexUser[] vtexUser = null;

            try
            {
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri($"http://{this._httpContextAccessor.HttpContext.Request.Headers[TaxjarConstants.VTEX_ACCOUNT_HEADER_NAME]}.{TaxjarConstants.ENVIRONMENT}.com.br/api/dataentities/CL/search?email={email}")
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
                    vtexUser = JsonConvert.DeserializeObject<VtexUser[]>(responseContent);
                }
                else
                {
                    _context.Vtex.Logger.Warn("GetShopperByEmail", null, $"Did not get shopper for email '{email}'");
                }
            }
            catch (Exception ex)
            {
                _context.Vtex.Logger.Error("GetShopperByEmail", null, $"Error getting shopper for email '{email}'", ex);
            }

            return vtexUser;
        }

        public async Task<string> GetShopperIdByEmail(string email)
        {
            string id = string.Empty;
            VtexUser[] vtexUsers = await this.GetShopperByEmail(email);
            if(vtexUsers.Length > 0)
            {
                id = vtexUsers[0].Id;
            }

            return id;
        }

        public async Task<VtexUser[]> GetShopperById(string userId)
        {

            VtexUser[] vtexUser = null;

            try
            {
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri($"http://{this._httpContextAccessor.HttpContext.Request.Headers[TaxjarConstants.VTEX_ACCOUNT_HEADER_NAME]}.{TaxjarConstants.ENVIRONMENT}.com.br/api/dataentities/CL/search?id={userId}")
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
                    vtexUser = JsonConvert.DeserializeObject<VtexUser[]>(responseContent);
                }
                else
                {
                    _context.Vtex.Logger.Warn("GetShopperById", null, $"Did not get shopper for id '{userId}'");
                }
            }
            catch (Exception ex)
            {
                _context.Vtex.Logger.Error("GetShopperById", null, $"Error getting shopper for id '{userId}'", ex);
            }

            return vtexUser;
        }

        public async Task<string> GetShopperEmailById(string userId)
        {
            string email = string.Empty;
            VtexUser[] vtexUsers = await this.GetShopperById(userId);
            if (vtexUsers.Length > 0)
            {
                email = vtexUsers[0].Email;
            }

            return email;
        }

        public async Task<GetListOfUsers> GetListOfUsers(int numItems, int pageNumber)
        {
            GetListOfUsers getListOfUsers = null;
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"http://{this._httpContextAccessor.HttpContext.Request.Headers[TaxjarConstants.VTEX_ACCOUNT_HEADER_NAME]}.vtexcommercestable.com.br/api/license-manager/site/pvt/logins/list/paged?numItems={numItems}&pageNumber={pageNumber}&sort=name&sortType=ASC'")
            };

            request.Headers.Add(TaxjarConstants.USE_HTTPS_HEADER_NAME, "true");
            //string authToken = this._httpContextAccessor.HttpContext.Request.Headers[TaxjarConstants.HEADER_VTEX_CREDENTIAL];
            string authToken = _context.Vtex.AdminUserAuthToken;
            if (authToken != null)
            {
                request.Headers.Add(TaxjarConstants.AUTHORIZATION_HEADER_NAME, authToken);
                request.Headers.Add(TaxjarConstants.VTEX_ID_HEADER_NAME, authToken);
                request.Headers.Add(TaxjarConstants.PROXY_AUTHORIZATION_HEADER_NAME, authToken);
            }

            var client = _clientFactory.CreateClient();
            var response = await client.SendAsync(request);
            string responseContent = await response.Content.ReadAsStringAsync();
            //Console.WriteLine($" - GetListOfUsers - [{response.StatusCode}] - '{responseContent}' - ");
            if (response.IsSuccessStatusCode)
            {
                getListOfUsers = JsonConvert.DeserializeObject<GetListOfUsers>(responseContent);
            }

            return getListOfUsers;
        }

        public async Task<NexusRegionsResponse> NexusRegions()
        {
            //Response.Headers.Add("Cache-Control", "private");
            NexusRegionsResponse nexusRegionsResponse = null;
            NexusRegionsStorage nexusRegionsStorage = await _taxjarRepository.GetNexusRegions();
            if(nexusRegionsStorage != null)
            {
                TimeSpan ts = DateTime.Now - nexusRegionsStorage.UpdatedAt;
                if(ts.Minutes > 10)
                {
                    nexusRegionsResponse = await _taxjarService.ListNexusRegions();
                    if(nexusRegionsResponse != null)
                    {
                        nexusRegionsStorage = new NexusRegionsStorage
                        {
                            UpdatedAt = DateTime.Now,
                            NexusRegionsResponse = nexusRegionsResponse
                        };

                        _taxjarRepository.SetNexusRegions(nexusRegionsStorage);
                    }
                }
                else
                {
                    //Console.WriteLine("Loaded Nexus from Storage");
                    nexusRegionsResponse = nexusRegionsStorage.NexusRegionsResponse;
                }
            }
            else
            {
                nexusRegionsResponse = await _taxjarService.ListNexusRegions();
                if (nexusRegionsResponse != null)
                {
                    nexusRegionsStorage = new NexusRegionsStorage
                    {
                        UpdatedAt = DateTime.Now,
                        NexusRegionsResponse = nexusRegionsResponse
                    };

                    _taxjarRepository.SetNexusRegions(nexusRegionsStorage);
                }
            }

            return nexusRegionsResponse;
        }

        public string GetCountryCode(string country)
        {
            try
            {
                if (country.Length > 2)
                {
                    country = TaxjarConstants.CountryCodesMapping[country];
                }
            }
            catch(Exception ex)
            {
                _context.Vtex.Logger.Error("GetCountryCode", null, $"Error getting country code {country}", ex);
            }

            return country;
        }
    }
}
