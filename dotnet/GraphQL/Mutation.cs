using GraphQL;
using GraphQL.Types;
using Taxjar.Data;
using Taxjar.Services;
using Taxjar.Models;
using Taxjar.GraphQL.Types;
using System;

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
                    if (customer.CustomerId.Contains("@"))
                    {
                        customer.CustomerId = vtexAPIService.GetShopperIdByEmail(customer.CustomerId).Result;
                    }

                    CustomerResponse taxjarResponse = taxjarService.CreateCustomer(customer).Result;
                    if (taxjarResponse != null)
                        created = true;

                    return created;
                });

            Field<BooleanGraphType>(
                "deleteCustomer",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "customerId", Description = "Customer Id" }
                ),
                resolve: context =>
                {
                    bool deleted = false;
                    string customerId = context.GetArgument<string>("customerId");
                    if (customerId.Contains("@"))
                    {
                        customerId = vtexAPIService.GetShopperIdByEmail(customerId).Result;
                    }

                    CustomerResponse taxjarResponse = taxjarService.DeleteCustomer(customerId).Result;
                    if (taxjarResponse != null)
                        deleted = true;

                    return deleted;
                });

            Field<StringGraphType>(
                "initConfiguration",
                resolve: context =>
                {
                    return vtexAPIService.InitConfiguration();
                });

            Field<StringGraphType>(
                "removeConfiguration",
                resolve: context =>
                {
                    return vtexAPIService.RemoveConfiguration();
                });
        }
    }
}