using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Taxjar.Models
{
    public class VtexTaxRequest
    {
        [JsonProperty("orderFormId")]
        public string OrderFormId { get; set; }

        [JsonProperty("salesChannel")]
        public string SalesChannel { get; set; }

        [JsonProperty("items")]
        public Item[] Items { get; set; }

        [JsonProperty("totals")]
        public Total[] Totals { get; set; }

        [JsonProperty("clientEmail")]
        public string ClientEmail { get; set; }

        [JsonProperty("shippingDestination")]
        public ShippingDestination ShippingDestination { get; set; }

        [JsonProperty("clientData")]
        public ClientData ClientData { get; set; }

        [JsonProperty("paymentData")]
        public PaymentData PaymentData { get; set; }

        //public override bool Equals(object obj)
        //{
        //    var other = obj as VtexTaxRequest;

        //    if (other == null)
        //        return false;

        //    if (OrderFormId != other.OrderFormId || SalesChannel != other.SalesChannel)
        //        return false;

        //    return true;
        //}
    }

    public class ClientData
    {
        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("document")]
        public string Document { get; set; }

        [JsonProperty("corporateDocument")]
        public object CorporateDocument { get; set; }

        [JsonProperty("stateInscription")]
        public object StateInscription { get; set; }
    }

    public class Item
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("sku")]
        public string Sku { get; set; }

        [JsonProperty("ean")]
        public string Ean { get; set; }

        [JsonProperty("refId")]
        public string RefId { get; set; }

        [JsonProperty("unitMultiplier")]
        public long UnitMultiplier { get; set; }

        [JsonProperty("measurementUnit")]
        public string MeasurementUnit { get; set; }

        [JsonProperty("targetPrice")]
        public decimal TargetPrice { get; set; }

        [JsonProperty("itemPrice")]
        public decimal ItemPrice { get; set; }

        [JsonProperty("quantity")]
        public long Quantity { get; set; }

        [JsonProperty("discountPrice")]
        public decimal DiscountPrice { get; set; }

        [JsonProperty("dockId")]
        public string DockId { get; set; }

        [JsonProperty("freightPrice")]
        public decimal FreightPrice { get; set; }

        [JsonProperty("brandId")]
        public string BrandId { get; set; }
    }

    public class PaymentData
    {
        [JsonProperty("payments")]
        public Payment[] Payments { get; set; }
    }

    public class Payment
    {
        [JsonProperty("paymentSystem")]
        public string PaymentSystem { get; set; }

        [JsonProperty("bin")]
        public object Bin { get; set; }

        [JsonProperty("referenceValue")]
        public decimal ReferenceValue { get; set; }

        [JsonProperty("value")]
        public decimal Value { get; set; }

        [JsonProperty("installments")]
        public object Installments { get; set; }
    }

    public class ShippingDestination
    {
        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("neighborhood")]
        public string Neighborhood { get; set; }

        [JsonProperty("postalCode")]
        public string PostalCode { get; set; }

        [JsonProperty("street")]
        public string Street { get; set; }
    }

    public class Total
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("value")]
        public decimal Value { get; set; }
    }
}

