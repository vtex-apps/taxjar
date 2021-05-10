using Taxjar.Data;
using Taxjar.Models;
using Taxjar.Services;
using GraphQL;
using GraphQL.Types;
using System.Linq;
using Taxjar.GraphQL.Types;
using System.Collections.Generic;
using System;

namespace Taxjar.GraphQL
{
    [GraphQLMetadata("Query")]
    public class Query : ObjectGraphType<object>
    {
        public Query(ITaxjarService taxjarService, ITaxjarRepository taxjarRepository, IVtexAPIService vtexAPIService)
        {
            Name = "Query";

            FieldAsync<ListGraphType<CustomerType>>(
                "listCustomers",
                resolve: async context =>
                {
                    List<Customer> customerList = new List<Customer>();
                    CustomersResponse customersResponse = await taxjarService.ListCustomers();
                    foreach(string customer in customersResponse.Customers)
                    {
                        CustomerResponse customerResponse = await taxjarService.ShowCustomer(customer);
                        if (customerResponse != null)
                        {
                            customerResponse.Customer.CustomerId = vtexAPIService.GetShopperEmailById(customerResponse.Customer.CustomerId).Result;
                            customerList.Add(customerResponse.Customer);
                        }
                        else
                        {
                            Console.WriteLine($"Could not load customer '{customer}'");
                        }
                    }

                    return customerList;
                }
            );

            FieldAsync<ListGraphType<CategoryType>>(
                "findProductCode",
                arguments: new QueryArguments(
                    new QueryArgument<StringGraphType> { Name = "searchTerm", Description = "Search term" }
                    ),
                resolve: async context =>
                {
                    string searchTerm = context.GetArgument<string>("searchTerm");
                    var categoriesAll = await taxjarService.Categories();
                    List<Category> categories  = categoriesAll.Categories.Where(c => c.Name.Contains(searchTerm)).ToList();

                    return categories;
                }
            );
        }
    }
}