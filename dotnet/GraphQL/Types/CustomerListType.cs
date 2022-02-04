using GraphQL;
using GraphQL.Types;
using Taxjar.Models;

namespace Taxjar.GraphQL.Types
{
    [GraphQLMetadata("CustomersResponse")]
    public class CustomerListType : ObjectGraphType<CustomersResponse>
    {
        public CustomerListType()
        {
            Name = "CustomersResponse";
            Field(c => c.Customers, type: typeof(ListGraphType<CustomerType>)).Description("List of Customers.");
        }
    }
}
