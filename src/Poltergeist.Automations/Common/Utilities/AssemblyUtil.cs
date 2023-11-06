namespace Poltergeist.Common.Utilities;

public static class AssemblyUtil
{
    /// <summary>
    /// Gets all types in the current assembly that are assignable to the specified types.
    /// </summary>
    /// <param name="types">An array of types to search.</param>
    /// <returns>An enumerable of types.</returns>
    public static IEnumerable<Type> GetSubclasses(params Type[] types)
    {
        return types.SelectMany(baseType => baseType.IsInterface
           ? baseType.Assembly.GetTypes().Where(t => t.IsClass && baseType.IsAssignableFrom(t))
           : baseType.Assembly.GetTypes().Where(t => t.IsClass && t.IsSubclassOf(baseType))
            );
    }

    /// <summary>
    /// Gets all types in the current assembly that are assignable to the specified type.
    /// </summary>
    /// <typeparam name="TBaseType">The base type to search.</typeparam>
    /// <returns>An enumerable of types.</returns>
    public static IEnumerable<Type> GetSubclasses<TBaseType>()
    {
        var baseType = typeof(TBaseType);
        return baseType.Assembly
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(t => t.IsSubclassOf(baseType) || baseType.IsAssignableFrom(t));
    }
    
    public static IEnumerable<Type> GetParentTypes(Type type)
    {
        var baseType = type.BaseType;
        while (baseType != null && baseType.Name != "Object")
        {
            yield return baseType;
            baseType = baseType.BaseType;
        }
    }

}
