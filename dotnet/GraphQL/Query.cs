using Taxjar.Data;
using Taxjar.Models;
using Taxjar.Services;
using GraphQL;
using GraphQL.Types;
using System.Linq;
using Taxjar.GraphQL.Types;

namespace Taxjar.GraphQL
{
    [GraphQLMetadata("Query")]
    public class Query : ObjectGraphType<object>
    {
        public Query(ITaxjarService taxjarService, ITaxjarRepository taxjarRepository, IVtexAPIService vtexAPIService)
        {
            Name = "Query";

            FieldAsync<CustomerListType>(
                "listCustomers",
                resolve: async context =>
                {
                    return await taxjarService.ListCustomers();
                }
            );
        }
    }
}