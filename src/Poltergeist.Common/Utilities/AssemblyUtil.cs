using System;
using System.Collections.Generic;
using System.Linq;

namespace Poltergeist.Common.Utilities;

public static class AssemblyUtil
{

    public static IEnumerable<Type> GetSubclasses(params Type[] types)
    {
        return types.SelectMany(baseType => baseType.IsInterface
           ? baseType.Assembly.GetTypes().Where(t => t.IsClass && baseType.IsAssignableFrom(t))
           : baseType.Assembly.GetTypes().Where(t => t.IsClass && t.IsSubclassOf(baseType))
            );
    }

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
