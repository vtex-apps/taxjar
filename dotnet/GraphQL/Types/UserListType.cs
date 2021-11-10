using GraphQL;
using GraphQL.Types;
using Taxjar.Models;

namespace Taxjar.GraphQL.Types
{
    [GraphQLMetadata("UserList")]
    public class UserListType : ObjectGraphType<GetListOfUsers>
    {
        public UserListType()
        {
            Name = "UserList";
            Field(u => u.Items, type: typeof(ListGraphType<UserItemType>)).Description("User Item");
            Field(u => u.Paging, type: typeof(PagingType)).Description("Paging");
        }
    }
}
