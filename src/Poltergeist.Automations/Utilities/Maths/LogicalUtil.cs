using System.Collections;

namespace Poltergeist.Automations.Utilities.Maths;

public static class LogicalUtil
{
    public static bool IsTruthy(object? value)
    {
        return value switch
        {
            null => false,
            Exception => false,
            bool b => b,
            string s => !string.IsNullOrEmpty(s),
            Array a => a.Length > 0,
            IEnumerable e => e.GetEnumerator().MoveNext(),
            var x when x.GetType().IsValueType => !x.Equals(Activator.CreateInstance(x.GetType())),
            _ => true,
        };
    }

}
