﻿using System.Reflection;

namespace Poltergeist.Operations;

public class InputOptions
{
    public override string ToString()
    {
        var list = new List<string>();
        var properties = GetType().GetProperties(BindingFlags.Public);
        foreach (var prop in properties)
        {
            var value = prop.GetValue(this);
            if(value != null)
            {
                list.Add($"{prop.Name} = {value}");
            }
        }
        return "{ " + string.Join(", ", list) + " }";
    }
}
