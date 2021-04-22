using GraphQL;
using GraphQL.Types;
using Taxjar.Data;
using Taxjar.Services;
using Taxjar.Models;
using Taxjar.GraphQL.Types;

namespace Taxjar.GraphQL
{
    [GraphQLMetadata("Mutation")]
    public class Mutation : ObjectGraphType<object>
    {
        public Mutation(ITaxjarService taxjarService, ITaxjarRepository taxjarRepository, IVtexAPIService vtexAPIService)
        {
            Name = "Mutation";

            Field<BooleanGraphType>(
                "createCustomer",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<CustomerInputType>> { Name = "customer", Description = "Customer" }
                ),
                resolve: context =>
                {
                    bool created = false;
                    Customer customer = context.GetArgument<Customer>("customer");
                    CustomerResponse taxjarResponse = taxjarService.CreateCustomer(customer).Result;
                    if (taxjarResponse != null)
                        created = true;

                    return created;
                });
        }
    }
}