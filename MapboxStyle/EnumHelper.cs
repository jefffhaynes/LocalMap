using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace MapboxStyle
{
    public static class EnumHelper<T>
    {
        private static readonly Dictionary<string, T> Values;

        static EnumHelper()
        {
            var type = typeof(T);
            var values = Enum.GetValues(type).Cast<T>();
            Values = values.ToDictionary(
                value => type.GetMember(value.ToString()).First().GetCustomAttribute<EnumMemberAttribute>().Value,
                value => value);
        }

        public static T Parse(string value)
        {
            return Values[value];
        }
    }
}
