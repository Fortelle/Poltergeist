using System.Reflection;

namespace Poltergeist.Automations.Utilities;

public static class ReflectionUtil
{
    public static void CopyProperties<T>(T target, T source)
    {
        var type = typeof(T);
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.Instance);

        foreach (var property in properties)
        {
            if (!property.CanRead || !property.CanWrite)
            {
                continue;
            }
            var value = property.GetValue(source, null);
            property.SetValue(target, value, null);
        }
    }

    public static void CopyNonNullProperties<T>(T target, T source)
    {
        var type = typeof(T);
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.Instance);

        foreach (var property in properties)
        {
            if (!property.CanRead || !property.CanWrite)
            {
                continue;
            }
            var value = property.GetValue(source, null);
            if (value is null)
            {
                continue;
            }
            property.SetValue(target, value, null);
        }
    }

    public static T Clone<T>(T source)
    {
        var type = typeof(T);
        var target = Activator.CreateInstance(type);
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.Instance);

        foreach (var property in properties)
        {
            if (!property.CanRead || !property.CanWrite)
            {
                continue;
            }
            var value = property.GetValue(source, null);
            property.SetValue(target, value, null);
        }

        return (T)target!;
    }
}
