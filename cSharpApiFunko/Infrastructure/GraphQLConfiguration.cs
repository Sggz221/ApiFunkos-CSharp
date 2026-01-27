using cSharpApiFunko.Errors;
using cSharpApiFunko.GraphQl;
using cSharpApiFunko.GraphQl.Mutation;
using cSharpApiFunko.GraphQl.Query;
using cSharpApiFunko.GraphQl.Types;

namespace cSharpApiFunko.Infrastructure;

/// <summary>
/// Configuración del servidor GraphQL
/// </summary>
public static class GraphQLConfiguration
{
    /// <summary>
    /// Configura el servidor GraphQL con queries, mutations, subscriptions y tipos personalizados
    /// </summary>
    public static IServiceCollection AddGraphQLConfiguration(this IServiceCollection services)
    {
        services
            .AddGraphQLServer()
            .AddQueryType<FunkoQuery>()
            .AddMutationType<FunkoMutation>()
            .AddSubscriptionType<FunkoSubscription>()
            .AddType<FunkoType>()
            // .AddType<CategoryType>()
            .AddType<FunkoError>()       
            .AddType<NotFoundError>()
            .AddType<ValidationError>()
            .AddType<ConflictError>()
            .AddType<StorageError>()
            .AddInMemorySubscriptions()
            .AddAuthorization()
            .AddFiltering()
            .AddSorting()
            .AddProjections();
        
        return services;
    }
}
