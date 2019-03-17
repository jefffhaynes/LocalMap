using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MapboxStyle
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum FilterOperator
    {
        [EnumMember(Value = "has")]
        Exists,

        [EnumMember(Value = "!has")]
        DoesNotExist,

        [EnumMember(Value = "==")]
        Equal,

        [EnumMember(Value = "!=")]
        NotEqual,

        [EnumMember(Value = ">")]
        GreaterThan,

        [EnumMember(Value = ">=")]
        GreaterThanOrEqual,

        [EnumMember(Value = "<")]
        LessThan,

        [EnumMember(Value = "<=")]
        LessThanOrEqual,

        [EnumMember(Value = "in")]
        Inclusion,

        [EnumMember(Value = "!in")]
        Exclusion,

        [EnumMember(Value = "all")]
        All,

        [EnumMember(Value = "any")]
        Any,

        [EnumMember(Value = "none")]
        None
    }
}