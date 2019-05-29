using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MapboxStyle.Expressions
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ExpressionOperator
    {
        [EnumMember(Value = "array")]
        Array,

        [EnumMember(Value = "boolean")]
        Boolean,

        [EnumMember(Value = "collator")]
        Collator,

        [EnumMember(Value = "format")]
        Format,

        [EnumMember(Value = "literal")]
        Literal,

        [EnumMember(Value = "number")]
        Number,

        [EnumMember(Value = "number-format")]
        NumberFormat,

        [EnumMember(Value = "object")]
        Object,

        [EnumMember(Value = "string")]
        String,

        [EnumMember(Value = "to-boolean")]
        ToBoolean,

        [EnumMember(Value = "to-color")]
        ToColor,

        [EnumMember(Value = "to-number")]
        ToNumber,

        [EnumMember(Value = "to-string")]
        ToString,

        [EnumMember(Value = "typeof")]
        TypeOf,

        [EnumMember(Value = "accumulated")]
        Accumulated,

        [EnumMember(Value = "feature-state")]
        FeatureState,

        [EnumMember(Value = "geometry-type")]
        GeometryType,

        [EnumMember(Value = "id")]
        Id,

        [EnumMember(Value = "line-progress")]
        LineProgress,

        [EnumMember(Value = "properties")]
        Properties,

        [EnumMember(Value = "at")]
        At,

        [EnumMember(Value = "get")]
        Get,

        [EnumMember(Value = "has")]
        Has,

        [EnumMember(Value = "length")]
        Length,

        [EnumMember(Value = "!")]
        Not,

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

        [EnumMember(Value = "all")]
        All,

        [EnumMember(Value = "any")]
        Any,

        [EnumMember(Value = "case")]
        Case,

        [EnumMember(Value = "coalesce")]
        Coalesce,

        [EnumMember(Value = "match")]
        Match,

        [EnumMember(Value = "interpolate")]
        Interpolate,

        [EnumMember(Value = "interpolate-hcl")]
        InterpolateHcl,

        [EnumMember(Value = "interpolate-lab")]
        InterpolateLab,

        [EnumMember(Value = "step")]
        Step,

        [EnumMember(Value = "let")]
        Let,

        [EnumMember(Value = "var")]
        Var,

        [EnumMember(Value = "concat")]
        Concat,

        [EnumMember(Value = "downcase")]
        Downcase,

        [EnumMember(Value = "is-supported-script")]
        IsSupportedScript,

        [EnumMember(Value = "resolved-locale")]
        ResolvedLocale,

        [EnumMember(Value = "upcase")]
        Upcase,

        [EnumMember(Value = "rgb")]
        Rgb,

        [EnumMember(Value = "rgba")]
        Rgba,

        [EnumMember(Value = "to-rgba")]
        ToRgba,

        [EnumMember(Value = "-")]
        Subtract,

        [EnumMember(Value = "*")]
        Multiply,

        [EnumMember(Value = "/")]
        Divide,

        [EnumMember(Value = "%")]
        Modulo,

        [EnumMember(Value = "^")]
        Power,

        [EnumMember(Value = "+")]
        Add,

        [EnumMember(Value = "abs")]
        Abs,

        [EnumMember(Value = "acos")]
        Acos,

        [EnumMember(Value = "asin")]
        Asin,

        [EnumMember(Value = "atan")]
        Atan,

        [EnumMember(Value = "ceil")]
        Ceiling,

        [EnumMember(Value = "cos")]
        Cos,

        [EnumMember(Value = "e")]
        E,

        [EnumMember(Value = "floor")]
        Floor,

        [EnumMember(Value = "ln")]
        Ln,

        [EnumMember(Value = "ln2")]
        Ln2,

        [EnumMember(Value = "log10")]
        Log10,

        [EnumMember(Value = "log2")]
        Log2,

        [EnumMember(Value = "max")]
        Max,

        [EnumMember(Value = "min")]
        Min,

        [EnumMember(Value = "pi")]
        Pi,

        [EnumMember(Value = "round")]
        Round,

        [EnumMember(Value = "sin")]
        Sin,

        [EnumMember(Value = "sqrt")]
        Sqrt,

        [EnumMember(Value = "tan")]
        Tan,

        [EnumMember(Value = "zoom")]
        Zoom,

        [EnumMember(Value = "heatmap-density")]
        HeatmapDensity
    }
}
