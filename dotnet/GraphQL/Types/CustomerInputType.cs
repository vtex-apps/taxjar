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
            Field(c => c.CustomerId);
            Field(c => c.ExemptionType);
            Field(c => c.Name);
            //Field(c => c.ExemptRegions);
            //Field<ListGraphType<ExemptRegionsType>>("ExemptRegions");
            Field(c => c.ExemptRegions, type: typeof(ListGraphType<ExemptRegionsType>)).Description("Exempt Regions");
        }
    }
}
