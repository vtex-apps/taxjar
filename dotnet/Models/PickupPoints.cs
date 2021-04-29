using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Taxjar.Models
{
    public class PickupPoints
    {
        [JsonProperty("items")]
        public PickupPointItem[] Items { get; set; }

        [JsonProperty("paging")]
        public PickupPointPaging Paging { get; set; }
    }

    public class PickupPointItem
    {
        [JsonProperty("id")]
        public string PickupPointId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public object Description { get; set; }

        [JsonProperty("instructions")]
        public string Instructions { get; set; }

        [JsonProperty("formatted_address")]
        public string FormattedAddress { get; set; }

        [JsonProperty("address")]
        public PickupPointAddress Address { get; set; }

        [JsonProperty("isActive")]
        public bool IsActive { get; set; }

        [JsonProperty("distance")]
        public long Distance { get; set; }

        [JsonProperty("seller")]
        public string Seller { get; set; }

        [JsonProperty("_sort")]
        public long[] Sort { get; set; }

        [JsonProperty("businessHours")]
        public PickupPointBusinessHour[] BusinessHours { get; set; }

        [JsonProperty("tagsLabel")]
        public object[] TagsLabel { get; set; }

        [JsonProperty("pickupHolidays")]
        public object[] PickupHolidays { get; set; }

        [JsonProperty("isThirdPartyPickup")]
        public bool IsThirdPartyPickup { get; set; }

        [JsonProperty("accountOwnerName")]
        public string AccountOwnerName { get; set; }

        [JsonProperty("accountOwnerId")]
        public string AccountOwnerId { get; set; }

        [JsonProperty("accountGroupId")]
        public string AccountGroupId { get; set; }
    }

    public class PickupPointAddress
    {
        [JsonProperty("postalCode")]
        public string PostalCode { get; set; }

        [JsonProperty("country")]
        public PickupPointCountry Country { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("neighborhood")]
        public string Neighborhood { get; set; }

        [JsonProperty("street")]
        public string Street { get; set; }

        [JsonProperty("number")]
        public object Number { get; set; }

        [JsonProperty("complement")]
        public string Complement { get; set; }

        [JsonProperty("reference")]
        public string Reference { get; set; }

        [JsonProperty("location")]
        public PickupPointLocation Location { get; set; }
    }

    public class PickupPointCountry
    {
        [JsonProperty("acronym")]
        public string Acronym { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class PickupPointLocation
    {
        [JsonProperty("latitude")]
        public double Latitude { get; set; }

        [JsonProperty("longitude")]
        public double Longitude { get; set; }
    }

    public class PickupPointBusinessHour
    {
        [JsonProperty("dayOfWeek")]
        public long DayOfWeek { get; set; }

        [JsonProperty("openingTime")]
        public DateTimeOffset OpeningTime { get; set; }

        [JsonProperty("closingTime")]
        public DateTimeOffset ClosingTime { get; set; }
    }

    public class PickupPointPaging
    {
        [JsonProperty("page")]
        public long Page { get; set; }

        [JsonProperty("perPage")]
        public long PerPage { get; set; }

        [JsonProperty("total")]
        public long Total { get; set; }

        [JsonProperty("pages")]
        public long Pages { get; set; }
    }
}
