using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Taxjar.Models
{
    public class VtexOrderForm
    {
        [JsonProperty("orderFormId")]
        public string OrderFormId { get; set; }

        [JsonProperty("salesChannel")]
        public string SalesChannel { get; set; }

        [JsonProperty("loggedIn")]
        public bool LoggedIn { get; set; }

        [JsonProperty("isCheckedIn")]
        public bool IsCheckedIn { get; set; }

        [JsonProperty("storeId")]
        public object StoreId { get; set; }

        [JsonProperty("checkedInPickupPointId")]
        public object CheckedInPickupPointId { get; set; }

        [JsonProperty("allowManualPrice")]
        public bool AllowManualPrice { get; set; }

        [JsonProperty("canEditData")]
        public bool CanEditData { get; set; }

        [JsonProperty("userProfileId")]
        public string UserProfileId { get; set; }

        [JsonProperty("userType")]
        public string UserType { get; set; }

        [JsonProperty("ignoreProfileData")]
        public bool IgnoreProfileData { get; set; }

        [JsonProperty("value")]
        public long Value { get; set; }

        [JsonProperty("messages")]
        public List<object> Messages { get; set; }

        [JsonProperty("items")]
        public List<OrderformItem> Items { get; set; }

        [JsonProperty("selectableGifts")]
        public List<SelectableGift> SelectableGifts { get; set; }

        [JsonProperty("totalizers")]
        public List<Totalizer> Totalizers { get; set; }

        [JsonProperty("shippingData")]
        public ShippingData ShippingData { get; set; }

        [JsonProperty("clientProfileData")]
        public ClientProfileData ClientProfileData { get; set; }

        [JsonProperty("paymentData")]
        public PaymentData PaymentData { get; set; }

        [JsonProperty("marketingData")]
        public MarketingData MarketingData { get; set; }

        [JsonProperty("sellers")]
        public List<Seller> Sellers { get; set; }

        [JsonProperty("clientPreferencesData")]
        public ClientPreferencesData ClientPreferencesData { get; set; }

        [JsonProperty("commercialConditionData")]
        public object CommercialConditionData { get; set; }

        [JsonProperty("storePreferencesData")]
        public StorePreferencesData StorePreferencesData { get; set; }

        [JsonProperty("giftRegistryData")]
        public object GiftRegistryData { get; set; }

        [JsonProperty("openTextField")]
        public object OpenTextField { get; set; }

        [JsonProperty("invoiceData")]
        public object InvoiceData { get; set; }

        [JsonProperty("customData")]
        public object CustomData { get; set; }

        [JsonProperty("itemMetadata")]
        public ItemMetadata ItemMetadata { get; set; }

        [JsonProperty("hooksData")]
        public object HooksData { get; set; }

        [JsonProperty("ratesAndBenefitsData")]
        public RatesAndBenefitsData RatesAndBenefitsData { get; set; }

        [JsonProperty("subscriptionData")]
        public object SubscriptionData { get; set; }

        [JsonProperty("itemsOrdination")]
        public object ItemsOrdination { get; set; }
    }

    public class VtexSubscriptionKey
    {
        [JsonProperty("maximumNumberOfCharacters")]
        public long MaximumNumberOfCharacters { get; set; }

        [JsonProperty("domain")]
        public List<string> Domain { get; set; }
    }

    public class OrderformItem
    {
        [JsonProperty("uniqueId")]
        public string UniqueId { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("productId")]
        public string ProductId { get; set; }

        [JsonProperty("productRefId")]
        public string ProductRefId { get; set; }

        [JsonProperty("refId")]
        public string RefId { get; set; }

        [JsonProperty("ean")]
        public object Ean { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("skuName")]
        public string SkuName { get; set; }

        [JsonProperty("modalType")]
        public object ModalType { get; set; }

        [JsonProperty("parentItemIndex")]
        public object ParentItemIndex { get; set; }

        [JsonProperty("parentAssemblyBinding")]
        public object ParentAssemblyBinding { get; set; }

        [JsonProperty("assemblies")]
        public List<object> Assemblies { get; set; }

        [JsonProperty("priceValidUntil")]
        public DateTimeOffset PriceValidUntil { get; set; }

        [JsonProperty("tax")]
        public long Tax { get; set; }

        [JsonProperty("price")]
        public long Price { get; set; }

        [JsonProperty("listPrice")]
        public long ListPrice { get; set; }

        [JsonProperty("manualPrice")]
        public object ManualPrice { get; set; }

        [JsonProperty("manualPriceAppliedBy")]
        public object ManualPriceAppliedBy { get; set; }

        [JsonProperty("sellingPrice")]
        public long SellingPrice { get; set; }

        [JsonProperty("rewardValue")]
        public long RewardValue { get; set; }

        [JsonProperty("isGift")]
        public bool IsGift { get; set; }

        [JsonProperty("additionalInfo")]
        public AdditionalInfo AdditionalInfo { get; set; }

        [JsonProperty("preSaleDate")]
        public object PreSaleDate { get; set; }

        [JsonProperty("productCategoryIds")]
        public string ProductCategoryIds { get; set; }

        [JsonProperty("productCategories")]
        public Dictionary<string, string> ProductCategories { get; set; }

        [JsonProperty("quantity")]
        public long Quantity { get; set; }

        [JsonProperty("seller")]
        public string Seller { get; set; }

        [JsonProperty("sellerChain")]
        public List<string> SellerChain { get; set; }

        [JsonProperty("imageUrl")]
        public Uri ImageUrl { get; set; }

        [JsonProperty("detailUrl")]
        public string DetailUrl { get; set; }

        [JsonProperty("components")]
        public List<object> Components { get; set; }

        [JsonProperty("bundleItems")]
        public List<object> BundleItems { get; set; }

        [JsonProperty("attachments")]
        public List<object> Attachments { get; set; }

        [JsonProperty("attachmentOfferings")]
        public List<AttachmentOffering> AttachmentOfferings { get; set; }

        [JsonProperty("offerings")]
        public List<object> Offerings { get; set; }

        [JsonProperty("priceTags")]
        public List<PriceTag> PriceTags { get; set; }

        [JsonProperty("availability")]
        public string Availability { get; set; }

        [JsonProperty("measurementUnit")]
        public string MeasurementUnit { get; set; }

        [JsonProperty("unitMultiplier")]
        public long UnitMultiplier { get; set; }

        [JsonProperty("manufacturerCode")]
        public object ManufacturerCode { get; set; }

        [JsonProperty("priceDefinition")]
        public PriceDefinition PriceDefinition { get; set; }
    }

    public class PriceDefinition
    {
        [JsonProperty("calculatedSellingPrice")]
        public long CalculatedSellingPrice { get; set; }

        [JsonProperty("total")]
        public long Total { get; set; }

        [JsonProperty("sellingPrices")]
        public List<SellingPrice> SellingPrices { get; set; }
    }

    public class SellingPrice
    {
        [JsonProperty("value")]
        public long Value { get; set; }

        [JsonProperty("quantity")]
        public long Quantity { get; set; }
    }

    public class MarketingData
    {
        [JsonProperty("utmSource")]
        public object UtmSource { get; set; }

        [JsonProperty("utmMedium")]
        public object UtmMedium { get; set; }

        [JsonProperty("utmCampaign")]
        public object UtmCampaign { get; set; }

        [JsonProperty("utmipage")]
        public object Utmipage { get; set; }

        [JsonProperty("utmiPart")]
        public object UtmiPart { get; set; }

        [JsonProperty("utmiCampaign")]
        public object UtmiCampaign { get; set; }

        [JsonProperty("coupon")]
        public object Coupon { get; set; }

        [JsonProperty("marketingTags")]
        public List<object> MarketingTags { get; set; }
    }

    public class InstallmentOption
    {
        [JsonProperty("paymentSystem")]
        public string PaymentSystem { get; set; }

        [JsonProperty("bin")]
        public object Bin { get; set; }

        [JsonProperty("paymentName")]
        public object PaymentName { get; set; }

        [JsonProperty("paymentGroupName")]
        public object PaymentGroupName { get; set; }

        [JsonProperty("value")]
        public long Value { get; set; }

        [JsonProperty("installments")]
        public List<Installment> Installments { get; set; }
    }

    public class Installment
    {
        [JsonProperty("count")]
        public long Count { get; set; }

        [JsonProperty("hasInterestRate")]
        public bool HasInterestRate { get; set; }

        [JsonProperty("interestRate")]
        public long InterestRate { get; set; }

        [JsonProperty("value")]
        public long Value { get; set; }

        [JsonProperty("total")]
        public long Total { get; set; }

        [JsonProperty("sellerMerchantInstallments", NullValueHandling = NullValueHandling.Ignore)]
        public List<Installment> SellerMerchantInstallments { get; set; }

        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }
    }

    public class PaymentSystem
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("groupName")]
        public string GroupName { get; set; }

        [JsonProperty("validator")]
        public Dictionary<string, bool?> Validator { get; set; }

        [JsonProperty("stringId")]
        public string StringId { get; set; }

        [JsonProperty("template")]
        public string Template { get; set; }

        [JsonProperty("requiresDocument")]
        public bool RequiresDocument { get; set; }

        [JsonProperty("isCustom")]
        public bool IsCustom { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("requiresAuthentication")]
        public bool RequiresAuthentication { get; set; }

        [JsonProperty("dueDate")]
        public DateTimeOffset DueDate { get; set; }

        [JsonProperty("availablePayments")]
        public object AvailablePayments { get; set; }
    }

    public class MerchantSellerPayment
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("installments")]
        public long Installments { get; set; }

        [JsonProperty("referenceValue")]
        public long ReferenceValue { get; set; }

        [JsonProperty("value")]
        public long Value { get; set; }

        [JsonProperty("interestRate")]
        public long InterestRate { get; set; }

        [JsonProperty("installmentValue")]
        public long InstallmentValue { get; set; }
    }

    public class RateAndBenefitsIdentifier
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("featured")]
        public bool Featured { get; set; }

        [JsonProperty("description")]
        public object Description { get; set; }

        [JsonProperty("matchedParameters")]
        public MatchedParameters MatchedParameters { get; set; }

        [JsonProperty("additionalInfo")]
        public object AdditionalInfo { get; set; }
    }

    public class MatchedParameters
    {
    }

    public class SelectableGift
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("availableQuantity")]
        public long AvailableQuantity { get; set; }

        [JsonProperty("availableGifts")]
        public List<AvailableGift> AvailableGifts { get; set; }
    }

    public class AvailableGift
    {
        [JsonProperty("isSelected")]
        public bool IsSelected { get; set; }

        [JsonProperty("uniqueId")]
        public string UniqueId { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("productId")]
        public string ProductId { get; set; }

        [JsonProperty("productRefId")]
        public string ProductRefId { get; set; }

        [JsonProperty("refId")]
        public string RefId { get; set; }

        [JsonProperty("ean")]
        public object Ean { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("skuName")]
        public string SkuName { get; set; }

        [JsonProperty("modalType")]
        public object ModalType { get; set; }

        [JsonProperty("parentItemIndex")]
        public object ParentItemIndex { get; set; }

        [JsonProperty("parentAssemblyBinding")]
        public object ParentAssemblyBinding { get; set; }

        [JsonProperty("assemblies")]
        public List<object> Assemblies { get; set; }

        [JsonProperty("priceValidUntil")]
        public object PriceValidUntil { get; set; }

        [JsonProperty("tax")]
        public long Tax { get; set; }

        [JsonProperty("price")]
        public object Price { get; set; }

        [JsonProperty("listPrice")]
        public object ListPrice { get; set; }

        [JsonProperty("manualPrice")]
        public object ManualPrice { get; set; }

        [JsonProperty("manualPriceAppliedBy")]
        public object ManualPriceAppliedBy { get; set; }

        [JsonProperty("sellingPrice")]
        public object SellingPrice { get; set; }

        [JsonProperty("rewardValue")]
        public long RewardValue { get; set; }

        [JsonProperty("isGift")]
        public bool IsGift { get; set; }

        [JsonProperty("additionalInfo")]
        public AdditionalInfo AdditionalInfo { get; set; }

        [JsonProperty("preSaleDate")]
        public object PreSaleDate { get; set; }

        [JsonProperty("productCategoryIds")]
        public string ProductCategoryIds { get; set; }

        [JsonProperty("productCategories")]
        public object ProductCategories { get; set; }

        [JsonProperty("quantity")]
        public long Quantity { get; set; }

        [JsonProperty("seller")]
        public string Seller { get; set; }

        [JsonProperty("sellerChain")]
        public List<string> SellerChain { get; set; }

        [JsonProperty("imageUrl")]
        public Uri ImageUrl { get; set; }

        [JsonProperty("detailUrl")]
        public string DetailUrl { get; set; }

        [JsonProperty("components")]
        public List<object> Components { get; set; }

        [JsonProperty("bundleItems")]
        public List<object> BundleItems { get; set; }

        [JsonProperty("attachments")]
        public List<object> Attachments { get; set; }

        [JsonProperty("attachmentOfferings")]
        public List<object> AttachmentOfferings { get; set; }

        [JsonProperty("offerings")]
        public List<object> Offerings { get; set; }

        [JsonProperty("priceTags")]
        public List<PriceTag> PriceTags { get; set; }

        [JsonProperty("availability")]
        public string Availability { get; set; }

        [JsonProperty("measurementUnit")]
        public string MeasurementUnit { get; set; }

        [JsonProperty("unitMultiplier")]
        public long UnitMultiplier { get; set; }

        [JsonProperty("manufacturerCode")]
        public object ManufacturerCode { get; set; }

        [JsonProperty("priceDefinition")]
        public object PriceDefinition { get; set; }
    }

    public class Totalizer
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("value")]
        public long Value { get; set; }
    }
}
