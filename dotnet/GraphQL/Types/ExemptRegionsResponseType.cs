using GraphQL;
using GraphQL.Types;
using Taxjar.Models;

namespace Taxjar.GraphQL.Types
{
    [GraphQLMetadata("ExemptRegionsType")]
    public class ExemptRegionsResponseType : ObjectGraphType<ExemptRegion>
    {
        public ExemptRegionsResponseType()
        {
            Name = "ExemptRegions";
            Field(r => r.Country, nullable: true);
            Field(r => r.State, nullable: true);
        }
    }
}
