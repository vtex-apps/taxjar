using GraphQL;
using GraphQL.Types;
using Taxjar.Models;

namespace Taxjar.GraphQL.Types
{
    [GraphQLMetadata("UserItem")]
    public class UserItemType : ObjectGraphType<UserItem>
    {
        public UserItemType()
        {
            Name = "UserItem";
            Field(u => u.Email, type: typeof(StringGraphType)).Description("Email");
            Field(u => u.Name, type: typeof(StringGraphType)).Description("Name");
            Field(u => u.Id, type: typeof(StringGraphType)).Description("Id");
        }
    }
}
