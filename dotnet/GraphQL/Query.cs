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
        public Query(ITaxjarService taxjarService, ITaxjarRepository taxjarRepository, IVtexApiService vtexAPIService)
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

            FieldAsync<UserListType>(
                "getListOfUsers",
                arguments: new QueryArguments(
                    new QueryArgument<IntGraphType> { Name = "numItems", Description = "Number of Items" },
                    new QueryArgument<IntGraphType> { Name = "pageNumber", Description = "Page Number" }
                    ),
                resolve: async context =>
                {
                    int numItems = context.GetArgument<int>("numItems");
                    int pageNumber = context.GetArgument<int>("pageNumber");
                    GetListOfUsers getListOfUsers = await vtexAPIService.GetListOfUsers(numItems, pageNumber);
                    List<UserItem> userItemsFiltered = new List<UserItem>();
                    foreach(UserItem userItem in getListOfUsers.Items)
                    {
                        userItem.Id = vtexAPIService.GetShopperIdByEmail(userItem.Email).Result;
                        if(!string.IsNullOrWhiteSpace(userItem.Id))
                        {
                            userItemsFiltered.Add(userItem);
                        }
                    }

                    getListOfUsers.Items = userItemsFiltered;

                    return getListOfUsers;
                }
            );

            FieldAsync<BooleanGraphType>(
                "verifyEmail",
                arguments: new QueryArguments(
                    new QueryArgument<StringGraphType> { Name = "email", Description = "Email" }
                    ),
                resolve: async context =>
                {
                    string email = context.GetArgument<string>("email");
                    string userId = await vtexAPIService.GetShopperIdByEmail(email);

                    return !string.IsNullOrEmpty(userId);
                }
            );
        }
    }
}