using Taxjar.Data;
using Taxjar.Models;
using Taxjar.Services;
using GraphQL;
using GraphQL.Types;
using System.Linq;
using Taxjar.GraphQL.Types;
using System.Collections.Generic;

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
                        customerList.Add(customerResponse.Customer);
                    }

                    return customerList;
                }
            );
        }
    }
}