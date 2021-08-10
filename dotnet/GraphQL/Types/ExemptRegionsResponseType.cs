using GraphQL;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;
using Taxjar.Models;

namespace Taxjar.GraphQL.Types
{
    [GraphQLMetadata("ExemptRegionsType")]
    public class ExemptRegionsResponseType : ObjectGraphType<ExemptRegion>
    {
        public ExemptRegionsResponseType()
        {
            Name = "ExemptRegions";
            Field(r => r.Country);
            Field(r => r.State);
        }
    }
}
