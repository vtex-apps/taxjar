using Newtonsoft.Json;
using System.Collections.Generic;

namespace Taxjar.Models
{
    public class GetListOfUsers
    {
        [JsonProperty("items")]
        public List<UserItem> Items { get; set; }

        [JsonProperty("paging")]
        public Paging Paging { get; set; }
    }

    public class UserItem
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("isAdmin")]
        public bool IsAdmin { get; set; }

        [JsonProperty("isReliable")]
        public bool IsReliable { get; set; }

        [JsonProperty("isBlocked")]
        public bool IsBlocked { get; set; }

        [JsonProperty("roles")]
        public List<object> Roles { get; set; }

        [JsonProperty("accountNames")]
        public List<object> AccountNames { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class Paging
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
