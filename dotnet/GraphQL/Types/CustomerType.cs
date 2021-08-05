using GraphQL;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;
using Taxjar.Models;

namespace Taxjar.GraphQL.Types
{
    [GraphQLMetadata("Customer")]
    public class CustomerType : ObjectGraphType<Customer>
    {
        public CustomerType()
        {
            Name = "Customer";
            Field(c => c.CustomerId, type: typeof(StringGraphType)).Description("Customer Id");
            Field(c => c.Name, type: typeof(StringGraphType)).Description("Customer Name");
            Field(c => c.ExemptionType, type: typeof(StringGraphType)).Description("Exemption Type");
            Field(c => c.ExemptRegions, type: typeof(ListGraphType<ExemptRegionsResponseType>)).Description("Exempt Region Type");
        }
    }
}
