using GraphQL;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;
using Taxjar.Models;

namespace Taxjar.GraphQL.Types
{
    [GraphQLMetadata("ExemptRegionsType")]
    public class ExemptRegionsType : InputObjectGraphType<ExemptRegion>
    {
        public ExemptRegionsType()
        {
            Name = "ExemptRegions";
            Field(r => r.Country, nullable: true);
            Field(r => r.State, nullable: true);
        }
    }
}
