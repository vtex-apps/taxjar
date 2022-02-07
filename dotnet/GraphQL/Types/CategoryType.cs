using GraphQL;
using GraphQL.Types;
using Taxjar.Models;

namespace Taxjar.GraphQL.Types
{
    [GraphQLMetadata("Category")]
    public class CategoryType : ObjectGraphType<Category>
    {
        public CategoryType()
        {
            Name = "Category";
            Field(c => c.Description, type: typeof(StringGraphType)).Description("Category Description");
            Field(c => c.Name, type: typeof(StringGraphType)).Description("Category Name");
            Field(c => c.ProductTaxCode, type: typeof(StringGraphType)).Description("Product Tax Code");
        }
    }
}
