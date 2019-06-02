using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace MapboxStyle.Expressions
{
    public class GetExpression : Expression
    {
        public GetExpression(JArray parameters)
        {
            ParameterName = parameters.First.ToString();
        }

        public string ParameterName { get; }

        public override object Evaluate(FilterType featureType, string featureId, double zoom, IDictionary<string, string> featureProperties)
        {
            if (featureProperties.TryGetValue(ParameterName, out var value))
            {
                return value;
            }

            return null;
        }
    }


    public static class ExpressionFactory
    {
        public static Expression GetExpression(JToken token)
        {
            if (token is JArray array)
            {
                if (array.First.Type == JTokenType.String)
                {
                    try
                    {
                        var op = array.First.ToObject<ExpressionOperator>();
                        var parameters = array.SkipInto();
                        return GetExpression(op, parameters);
                    }
                    catch
                    {
                        return new ConstantExpression(array.ToArray());
                    }
                }

                return new ConstantExpression(array.ToArray());
            }

            return new ConstantExpression(token.ToObject<object>());
        }

        private static Expression GetExpression(ExpressionOperator op, JArray parameters)
        { 
            switch (op)
            {
                case ExpressionOperator.Array:
                    break;
                case ExpressionOperator.Boolean:
                    break;
                case ExpressionOperator.Collator:
                    break;
                case ExpressionOperator.Format:
                    break;
                case ExpressionOperator.Literal:
                    return new LiteralExpression(parameters);
                case ExpressionOperator.Number:
                    break;
                case ExpressionOperator.NumberFormat:
                    break;
                case ExpressionOperator.Object:
                    break;
                case ExpressionOperator.String:
                    break;
                case ExpressionOperator.ToBoolean:
                    break;
                case ExpressionOperator.ToColor:
                    break;
                case ExpressionOperator.ToNumber:
                    break;
                case ExpressionOperator.ToString:
                    break;
                case ExpressionOperator.TypeOf:
                    break;
                case ExpressionOperator.Accumulated:
                    break;
                case ExpressionOperator.FeatureState:
                    break;
                case ExpressionOperator.GeometryType:
                    return new GeometryTypeExpression();
                case ExpressionOperator.Id:
                    break;
                case ExpressionOperator.LineProgress:
                    break;
                case ExpressionOperator.Properties:
                    break;
                case ExpressionOperator.At:
                    break;
                case ExpressionOperator.Get:
                    return new GetExpression(parameters);
                case ExpressionOperator.Has:
                    break;
                case ExpressionOperator.Length:
                    break;
                case ExpressionOperator.Not:
                    break;
                case ExpressionOperator.Equal:
                    return new EqualExpression(parameters);
                case ExpressionOperator.NotEqual:
                    return new NotEqualExpression(parameters);
                case ExpressionOperator.GreaterThan:
                    return new GreaterThanExpression(parameters);
                case ExpressionOperator.GreaterThanOrEqual:
                    return new GreaterThanOrEqualExpression(parameters);
                case ExpressionOperator.LessThan:
                    return new LessThanExpression(parameters);
                case ExpressionOperator.LessThanOrEqual:
                    return new LessThanOrEqualExpression(parameters);
                case ExpressionOperator.All:
                    return new AllExpression(parameters);
                case ExpressionOperator.Any:
                    break;
                case ExpressionOperator.Case:
                    break;
                case ExpressionOperator.Coalesce:
                    return new CoalesceExpression(parameters);
                case ExpressionOperator.Match:
                    return new MatchExpression(parameters);
                case ExpressionOperator.Interpolate:
                    return new InterpolateExpression(parameters);
                case ExpressionOperator.InterpolateHcl:
                    break;
                case ExpressionOperator.InterpolateLab:
                    break;
                case ExpressionOperator.Step:
                    return new StepExpression(parameters);
                case ExpressionOperator.Let:
                    break;
                case ExpressionOperator.Var:
                    break;
                case ExpressionOperator.Concat:
                    break;
                case ExpressionOperator.Downcase:
                    break;
                case ExpressionOperator.IsSupportedScript:
                    break;
                case ExpressionOperator.ResolvedLocale:
                    break;
                case ExpressionOperator.Upcase:
                    break;
                case ExpressionOperator.Rgb:
                    break;
                case ExpressionOperator.Rgba:
                    break;
                case ExpressionOperator.ToRgba:
                    break;
                case ExpressionOperator.Subtract:
                    break;
                case ExpressionOperator.Multiply:
                    break;
                case ExpressionOperator.Divide:
                    break;
                case ExpressionOperator.Modulo:
                    break;
                case ExpressionOperator.Power:
                    break;
                case ExpressionOperator.Add:
                    break;
                case ExpressionOperator.Abs:
                    break;
                case ExpressionOperator.Acos:
                    break;
                case ExpressionOperator.Asin:
                    break;
                case ExpressionOperator.Atan:
                    break;
                case ExpressionOperator.Ceiling:
                    break;
                case ExpressionOperator.Cos:
                    break;
                case ExpressionOperator.E:
                    break;
                case ExpressionOperator.Floor:
                    break;
                case ExpressionOperator.Ln:
                    break;
                case ExpressionOperator.Ln2:
                    break;
                case ExpressionOperator.Log10:
                    break;
                case ExpressionOperator.Log2:
                    break;
                case ExpressionOperator.Max:
                    break;
                case ExpressionOperator.Min:
                    break;
                case ExpressionOperator.Pi:
                    break;
                case ExpressionOperator.Round:
                    break;
                case ExpressionOperator.Sin:
                    break;
                case ExpressionOperator.Sqrt:
                    break;
                case ExpressionOperator.Tan:
                    break;
                case ExpressionOperator.HeatmapDensity:
                    break;
                case ExpressionOperator.Zoom:
                    return new ZoomExpression();
                default:
                    throw new ArgumentOutOfRangeException(nameof(op), op, null);
            }

            throw new NotSupportedException();
        }
    }

    internal class CoalesceExpression : Expression
    {
        private readonly List<Expression> _expressions;

        public CoalesceExpression(JArray parameters)
        {
            _expressions = parameters.Select(ExpressionFactory.GetExpression).ToList();
        }

        public override object Evaluate(FilterType featureType, string featureId, double zoom, IDictionary<string, string> featureProperties)
        {
            throw new NotImplementedException();
        }
    }

    internal class StepExpression : Expression
    {
        private readonly Expression _inputExpression;
        private readonly Expression _baseOutput;
        private readonly List<Stop> _stops = new List<Stop>();

        public StepExpression(JArray parameters)
        {
            _inputExpression = ExpressionFactory.GetExpression(parameters.First);
            _baseOutput = ExpressionFactory.GetExpression(parameters.SkipInto().First);
            var stops = parameters.SkipInto(2);

            for (var index = 0; index < stops.Count / 2; index++)
            {
                var stopOffset = stops[index * 2].ToObject<double>();
                var stopValueToken = stops[index * 2 + 1];

                var stopExpression = ExpressionFactory.GetExpression(stopValueToken);
                _stops.Add(new Stop(stopOffset, stopExpression));
            }
        }

        public override object Evaluate(FilterType featureType, string featureId, double zoom, IDictionary<string, string> featureProperties)
        {
            throw new NotImplementedException();
        }
    }

    internal class LiteralExpression : Expression
    {
        private readonly object _value;

        public LiteralExpression(JArray parameters)
        {
            var value = parameters.First;
            switch (value.Type)
            {
                case JTokenType.Array:
                    _value = value.ToArray();
                    break;
                case JTokenType.String:
                    _value = value.ToString();
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        public override object Evaluate(FilterType featureType, string featureId, double zoom, IDictionary<string, string> featureProperties)
        {
            throw new NotImplementedException();
        }
    }

    internal class NotEqualExpression : EqualExpression
    {
        public NotEqualExpression(JArray parameters) : base(parameters)
        {
        }

        protected override bool Compare(object lhs, object rhs)
        {
            return !base.Compare(lhs, rhs);
        }
    }

    internal class GreaterThanExpression : NumericalComparisonExpression
    {
        public GreaterThanExpression(JArray parameters) : base(parameters)
        {
        }

        protected override bool Compare(double lhs, double rhs)
        {
            return lhs > rhs;
        }
    }

    internal class GreaterThanOrEqualExpression : NumericalComparisonExpression
    {
        public GreaterThanOrEqualExpression(JArray parameters) : base(parameters)
        {
        }

        protected override bool Compare(double lhs, double rhs)
        {
            return lhs >= rhs;
        }
    }

    internal class LessThanOrEqualExpression : NumericalComparisonExpression
    {
        public LessThanOrEqualExpression(JArray parameters) : base(parameters)
        {
        }

        protected override bool Compare(double lhs, double rhs)
        {
            return lhs <= rhs;
        }
    }


    internal class LessThanExpression : NumericalComparisonExpression
    {
        public LessThanExpression(JArray parameters) : base(parameters)
        {
        }

        protected override bool Compare(double lhs, double rhs)
        {
            return lhs < rhs;
        }
    }

    internal class GeometryTypeExpression : Expression
    {
        public override object Evaluate(FilterType featureType, string featureId, double zoom, IDictionary<string, string> featureProperties)
        {
            return featureType;
        }
    }

    internal class AllExpression : Expression
    {
        private readonly List<Expression> _expressions;

        public AllExpression(JArray parameters)
        {
            _expressions = parameters.Select(ExpressionFactory.GetExpression).ToList();
        }

        public override object Evaluate(FilterType featureType, string featureId, double zoom, IDictionary<string, string> featureProperties)
        {
            if (_expressions.Count == 0)
            {
                return true;
            }

            return _expressions.All(expression =>
                expression.GetBool(featureType, featureId, zoom, featureProperties));
        }
    }

    public abstract class NumericalComparisonExpression : ComparisonExpression
    {
        protected NumericalComparisonExpression(JArray parameters) : base(parameters)
        {
        }

        protected override bool Compare(object lhs, object rhs)
        {
            if (lhs == null || rhs == null)
            {
                return false;
            }

            if (double.TryParse(lhs.ToString(), out var lhsd) && double.TryParse(rhs.ToString(), out var rhsd))
            {
                return Compare(lhsd, rhsd);
            }

            return false;
        }

        protected abstract bool Compare(double lhs, double rhs);
    }

    public abstract class ComparisonExpression : Expression
    {
        private readonly Expression _leftExpression;
        private readonly Expression _rightExpression;

        protected ComparisonExpression(JArray parameters)
        {
            _leftExpression = ExpressionFactory.GetExpression(parameters.First);
            _rightExpression = ExpressionFactory.GetExpression(parameters.Last);
        }

        public override object Evaluate(FilterType featureType, string featureId, double zoom, IDictionary<string, string> featureProperties)
        {
            var lhs = _leftExpression.Evaluate(featureType, featureId, zoom, featureProperties);
            var rhs = _rightExpression.Evaluate(featureType, featureId, zoom, featureProperties);

            return Compare(lhs, rhs);
        }

        protected abstract bool Compare(object lhs, object rhs);
    }

    public class EqualExpression : ComparisonExpression
    {
        public EqualExpression(JArray parameters) : base(parameters)
        {
        }

        protected override bool Compare(object lhs, object rhs)
        {
            if (lhs == null && rhs == null)
            {
                return true;
            }

            if (lhs == null || rhs == null)
            {
                return false;
            }

            return lhs.ToString() == rhs.ToString();
        }
    }

    public class ZoomExpression : Expression
    {
        public override object Evaluate(FilterType featureType, string featureId, double zoom, IDictionary<string, string> featureProperties)
        {
            return zoom;
        }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum EasingMode
    {
        [EnumMember(Value = "linear")]
        Linear,
        [EnumMember(Value = "exponential")]
        Exponential,
        [EnumMember(Value = "cubic-bezier")]
        CubicBezier
    }

    public abstract class EasingFunction
    {
    }

    public class LinearEase : EasingFunction
    {
    }

    public class ExponentialEase : EasingFunction
    {
        private double _base;

        public ExponentialEase(JArray array)
        {
            _base = array[0].ToObject<double>();
        }
    }

    public class CubicBezierEase : EasingFunction
    {
        public CubicBezierEase(JArray array)
        {
        }
    }

    public class Stop
    {
        public Stop(double offset, Expression expression)
        {
            Offset = offset;
            Expression = expression;
        }

        public double Offset { get; }

        public Expression Expression { get; }
    }

    public class ConstantExpression : Expression
    {
        public ConstantExpression(object value)
        {
            Value = value;
        }

        public object Value { get; }

        public override object Evaluate(FilterType featureType, string featureId, double zoom, IDictionary<string, string> featureProperties)
        {
            return Value;
        }
    }

    public class MatchExpression : Expression
    {
        private readonly Expression _matchingExpression;
        private readonly JArray _matchingArray;

        public MatchExpression(JArray parameters)
        {
            _matchingExpression = ExpressionFactory.GetExpression(parameters.First);
            _matchingArray = parameters.SkipInto();
        }

        public override object Evaluate(FilterType featureType, string featureId, double zoom, IDictionary<string, string> featureProperties)
        {
            var matchValue = _matchingExpression.Evaluate(featureType, featureId, zoom, featureProperties);
            foreach (var item in _matchingArray)
            {
                if (item == matchValue)
                {
                    return item;
                }

                return null;
            }

            return null;
        }
    }

    public class InterpolateExpression : Expression
    {
        public EasingFunction EasingFunction { get; }

        public Expression InputExpression { get; }

        public List<Stop> Stops { get; } = new List<Stop>();

        public InterpolateExpression(JArray array)
        {
            var interpolationFunctionValues = (JArray) array.First;
            var easingMode = interpolationFunctionValues.First.ToObject<EasingMode>();
            var easingModeParameters = interpolationFunctionValues.SkipInto();

            var input = (JArray) array.First.Next;
            
            InputExpression = ExpressionFactory.GetExpression(input);

            switch (easingMode)
            {
                case EasingMode.Linear:
                    EasingFunction = new LinearEase();
                    break;
                case EasingMode.Exponential:
                    EasingFunction = new ExponentialEase(easingModeParameters);
                    break;
                case EasingMode.CubicBezier:
                    EasingFunction = new CubicBezierEase(easingModeParameters);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var stopArray = array.SkipInto(2);

            for (var index = 0; index < stopArray.Count / 2; index++)
            {
                var stopOffset = stopArray[index * 2].ToObject<double>();
                var stopValueToken = stopArray[index * 2 + 1];

                var stopExpression = ExpressionFactory.GetExpression(stopValueToken);

                Stops.Add(new Stop(stopOffset, stopExpression));
            }
        }

        public override object Evaluate(FilterType featureType, string featureId, double zoom, IDictionary<string, string> featureProperties)
        {
            throw new NotImplementedException();
        }
    }
}
