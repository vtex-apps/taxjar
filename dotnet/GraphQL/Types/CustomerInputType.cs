using GraphQL;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;
using Taxjar.Models;

namespace Taxjar.GraphQL.Types
{
    [GraphQLMetadata("CustomerInputType")]
    public class CustomerInputType : InputObjectGraphType<Customer>
    {
        public CustomerInputType()
        {
            Name = "CustomerInput";
            // Field(c => c.CustomerId);
            // Field(c => c.ExemptionType);
            // Field(c => c.Name);
            //Field(c => c.ExemptRegions);
            //Field<ListGraphType<ExemptRegionsType>>("ExemptRegions");
            // Field(c => c.ExemptRegions, type: typeof(ListGraphType<ExemptRegionsType>)).Description("Exempt Regions");
            Field(c => c.CustomerId, type: typeof(StringGraphType)).Description("Customer Id");
            Field(c => c.Name, type: typeof(StringGraphType)).Description("Customer Name");
            Field(c => c.ExemptionType, type: typeof(StringGraphType)).Description("Exemption Type");
            Field(c => c.ExemptRegions, type: typeof(ListGraphType<ExemptRegionsType>)).Description("Exempt Regions");
        }
    }
}
