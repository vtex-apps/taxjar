using GraphQL;
using GraphQL.Types;
using Taxjar.Models;

namespace Taxjar.GraphQL.Types
{
    [GraphQLMetadata("Paging")]
    public class PagingType : ObjectGraphType<Paging>
    {
        public PagingType()
        {
            Name = "Paging";
            Field(p => p.Page, type: typeof(IntGraphType)).Description("Page");
            Field(p => p.Pages, type: typeof(IntGraphType)).Description("Pages");
            Field(p => p.PerPage, type: typeof(IntGraphType)).Description("PerPage");
            Field(p => p.Total, type: typeof(IntGraphType)).Description("Total");
        }
    }
}
