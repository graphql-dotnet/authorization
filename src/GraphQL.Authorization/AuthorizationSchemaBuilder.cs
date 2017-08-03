using System.Reflection;
using GraphQL.Types;
using GraphQL.Utilities;
using GraphQLParser.AST;

namespace GraphQL.Authorization
{
    public class AuthorizationSchemaBuilder : SchemaBuilder
    {
        protected override IObjectGraphType ToObjectGraphType(GraphQLObjectTypeDefinition astType)
        {
            var objectType = base.ToObjectGraphType(astType);
            var typeConfig = Types.For(objectType.Name);

            var attr = typeConfig.Type?.GetTypeInfo().GetCustomAttribute<GraphQLAuthorizeAttribute>();
            if (attr != null)
            {
                objectType.AuthorizeWith(attr.Policy);
            }

            return objectType;
        }

        protected override FieldType ToFieldType(string parentTypeName, GraphQLFieldDefinition fieldDef)
        {
            var fieldType = base.ToFieldType(parentTypeName, fieldDef);
            var typeConfig = Types.For(parentTypeName);

            var methodInfo = typeConfig.MethodForField(fieldType.Name);

            var attr = methodInfo?.GetCustomAttribute<GraphQLAuthorizeAttribute>();
            if (attr != null)
            {
                fieldType.AuthorizeWith(attr.Policy);
            }

            return fieldType;
        }
    }
}