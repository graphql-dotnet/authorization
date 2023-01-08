namespace GraphQL;

/// <summary>
/// CLR type to map to the 'Query' graph type.
/// </summary>
public class Query
{
    /// <summary>
    /// Resolver for 'Query.viewer' field.
    /// </summary>
    [Authorize("AdminPolicy")]
    public User Viewer() => new() { Id = Guid.NewGuid().ToString(), Name = "Quinn" };

    /// <summary>
    /// Resolver for 'Query.users' field.
    /// </summary>
    public List<User> Users() => new() { new User { Id = Guid.NewGuid().ToString(), Name = "Quinn" } };

    /// <summary>
    /// Resolver for 'Query.guest' field.
    /// </summary>
    public string Guest() => "guest42";
}

/// <summary>
/// CLR type to map to the 'User' graph type.
/// </summary>
public class User
{
    /// <summary>
    /// Resolver for 'User.id' field. Just a simple property.
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Resolver for 'User.name' field. Just a simple property.
    /// </summary>
    public string? Name { get; set; }
}
