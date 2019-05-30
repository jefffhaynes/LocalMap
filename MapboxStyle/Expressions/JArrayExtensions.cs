using System.Linq;
using Newtonsoft.Json.Linq;

namespace MapboxStyle.Expressions
{
    public static class JArrayExtensions
    {
        public static JArray SkipInto(this JArray array, int skip = 1)
        {
            return new JArray{array.Skip(skip)};
        }
    }
}
