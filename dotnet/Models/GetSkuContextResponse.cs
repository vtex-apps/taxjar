﻿using Newtonsoft.Json;
using System;

namespace Taxjar.Models
{
    public class GetSkuContextResponse
    {
        [JsonProperty("Id")]
        public long Id { get; set; }

        [JsonProperty("ProductId")]
        public long ProductId { get; set; }

        [JsonProperty("NameComplete")]
        public string NameComplete { get; set; }

        [JsonProperty("ProductName")]
        public string ProductName { get; set; }

        [JsonProperty("ProductDescription")]
        public string ProductDescription { get; set; }

        [JsonProperty("ProductRefId")]
        public string ProductRefId { get; set; }

        [JsonProperty("TaxCode")]
        public string TaxCode { get; set; }

        [JsonProperty("SkuName")]
        public string SkuName { get; set; }

        [JsonProperty("IsActive")]
        public bool IsActive { get; set; }

        [JsonProperty("IsTransported")]
        public bool IsTransported { get; set; }

        [JsonProperty("IsInventoried")]
        public bool IsInventoried { get; set; }

        [JsonProperty("IsGiftCardRecharge")]
        public bool IsGiftCardRecharge { get; set; }

        [JsonProperty("ImageUrl")]
        public Uri ImageUrl { get; set; }

        [JsonProperty("DetailUrl")]
        public string DetailUrl { get; set; }

        [JsonProperty("CSCIdentification")]
        public object CscIdentification { get; set; }

        [JsonProperty("BrandId")]
        public string BrandId { get; set; }

        [JsonProperty("BrandName")]
        public string BrandName { get; set; }

        [JsonProperty("IsBrandActive")]
        public bool IsBrandActive { get; set; }

        [JsonProperty("Dimension")]
        public Dimension Dimension { get; set; }

        [JsonProperty("RealDimension")]
        public RealDimension RealDimension { get; set; }

        [JsonProperty("ManufacturerCode")]
        public string ManufacturerCode { get; set; }

        [JsonProperty("IsKit")]
        public bool IsKit { get; set; }

        [JsonProperty("KitItems")]
        public object[] KitItems { get; set; }

        [JsonProperty("Services")]
        public object[] Services { get; set; }

        [JsonProperty("Categories")]
        public object[] Categories { get; set; }

        [JsonProperty("CategoriesFullPath")]
        public string[] CategoriesFullPath { get; set; }

        [JsonProperty("Attachments")]
        public object[] Attachments { get; set; }

        [JsonProperty("Collections")]
        public object[] Collections { get; set; }

        [JsonProperty("SkuSellers")]
        public SkuSeller[] SkuSellers { get; set; }

        [JsonProperty("SalesChannels")]
        public long[] SalesChannels { get; set; }

        [JsonProperty("Images")]
        public Image[] Images { get; set; }

        [JsonProperty("Videos")]
        public object[] Videos { get; set; }

        [JsonProperty("SkuSpecifications")]
        public Specification[] SkuSpecifications { get; set; }

        [JsonProperty("ProductSpecifications")]
        public Specification[] ProductSpecifications { get; set; }

        [JsonProperty("ProductClustersIds")]
        public string ProductClustersIds { get; set; }

        [JsonProperty("PositionsInClusters")]
        public PositionsInClusters PositionsInClusters { get; set; }

        [JsonProperty("ProductCategoryIds")]
        public string ProductCategoryIds { get; set; }

        [JsonProperty("IsDirectCategoryActive")]
        public bool IsDirectCategoryActive { get; set; }

        [JsonProperty("ProductGlobalCategoryId")]
        public string ProductGlobalCategoryId { get; set; }

        [JsonProperty("ProductCategories")]
        public object ProductCategories { get; set; }

        [JsonProperty("CommercialConditionId")]
        public object CommercialConditionId { get; set; }

        [JsonProperty("RewardValue")]
        public string RewardValue { get; set; }

        [JsonProperty("AlternateIds")]
        public AlternateIds AlternateIds { get; set; }

        [JsonProperty("AlternateIdValues")]
        public string[] AlternateIdValues { get; set; }

        [JsonProperty("EstimatedDateArrival")]
        public string EstimatedDateArrival { get; set; }

        [JsonProperty("MeasurementUnit")]
        public string MeasurementUnit { get; set; }

        [JsonProperty("UnitMultiplier")]
        public long UnitMultiplier { get; set; }

        [JsonProperty("InformationSource")]
        public string InformationSource { get; set; }

        [JsonProperty("ModalType")]
        public object ModalType { get; set; }

        [JsonProperty("KeyWords")]
        public string KeyWords { get; set; }

        [JsonProperty("ReleaseDate")]
        public string ReleaseDate { get; set; }

        [JsonProperty("ProductIsVisible")]
        public bool ProductIsVisible { get; set; }

        [JsonProperty("ShowIfNotAvailable")]
        public bool ShowIfNotAvailable { get; set; }

        [JsonProperty("IsProductActive")]
        public bool IsProductActive { get; set; }

        [JsonProperty("ProductFinalScore")]
        public long ProductFinalScore { get; set; }
    }

    public partial class AlternateIds
    {
        [JsonProperty("RefId")]
        public string RefId { get; set; }
    }

    public partial class Dimension
    {
        [JsonProperty("cubicweight")]
        public long Cubicweight { get; set; }

        [JsonProperty("height")]
        public long Height { get; set; }

        [JsonProperty("length")]
        public long Length { get; set; }

        [JsonProperty("weight")]
        public long Weight { get; set; }

        [JsonProperty("width")]
        public long Width { get; set; }
    }

    public partial class Image
    {
        [JsonProperty("ImageUrl")]
        public Uri ImageUrl { get; set; }

        [JsonProperty("ImageName")]
        public string ImageName { get; set; }

        [JsonProperty("FileId")]
        public long FileId { get; set; }
    }

    public partial class PositionsInClusters
    {
    }

    public class Specification
    {
        [JsonProperty("FieldId")]
        public long FieldId { get; set; }

        [JsonProperty("FieldName")]
        public string FieldName { get; set; }

        [JsonProperty("FieldValueIds")]
        public long[] FieldValueIds { get; set; }

        [JsonProperty("FieldValues")]
        public string[] FieldValues { get; set; }

        [JsonProperty("IsFilter")]
        public bool IsFilter { get; set; }

        [JsonProperty("FieldGroupId")]
        public long FieldGroupId { get; set; }

        [JsonProperty("FieldGroupName")]
        public string FieldGroupName { get; set; }
    }

    public partial class RealDimension
    {
        [JsonProperty("realCubicWeight")]
        public long RealCubicWeight { get; set; }

        [JsonProperty("realHeight")]
        public long RealHeight { get; set; }

        [JsonProperty("realLength")]
        public long RealLength { get; set; }

        [JsonProperty("realWeight")]
        public long RealWeight { get; set; }

        [JsonProperty("realWidth")]
        public long RealWidth { get; set; }
    }

    public partial class SkuSeller
    {
        [JsonProperty("SellerId")]
        public string SellerId { get; set; }

        [JsonProperty("StockKeepingUnitId")]
        public long StockKeepingUnitId { get; set; }

        [JsonProperty("SellerStockKeepingUnitId")]
        public string SellerStockKeepingUnitId { get; set; }

        [JsonProperty("IsActive")]
        public bool IsActive { get; set; }

        [JsonProperty("FreightCommissionPercentage")]
        public long FreightCommissionPercentage { get; set; }

        [JsonProperty("ProductCommissionPercentage")]
        public long ProductCommissionPercentage { get; set; }
    }
}
