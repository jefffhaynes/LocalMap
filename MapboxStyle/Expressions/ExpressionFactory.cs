using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace MapboxStyle.Expressions
{
    public class NumericExpressionFactory : ExpressionFactory<double>
    {
        protected override Expression<double> GetExpression(ExpressionOperator op, JArray parameters)
        {
            switch (op)
            {
                case ExpressionOperator.Zoom:
                    return new ZoomExpression();
                case ExpressionOperator.Interpolate:
                    return new NumericInterpolateExpression(parameters);
            }

            return base.GetExpression(op, parameters);
        }
    }

    public class ExpressionFactory<T>
    {
        public Expression<T> GetExpression(JArray array)
        {
            var op = array.First.ToObject<ExpressionOperator>();
            var parameters = array.SkipInto();
            return GetExpression(op, parameters);
        }

        protected virtual Expression<T> GetExpression(ExpressionOperator op, JArray parameters)
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
                    break;
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
                    break;
                case ExpressionOperator.Id:
                    break;
                case ExpressionOperator.LineProgress:
                    break;
                case ExpressionOperator.Properties:
                    break;
                case ExpressionOperator.At:
                    break;
                case ExpressionOperator.Get:
                    break;
                case ExpressionOperator.Has:
                    break;
                case ExpressionOperator.Length:
                    break;
                case ExpressionOperator.Not:
                    break;
                case ExpressionOperator.Equal:
                    break;
                case ExpressionOperator.NotEqual:
                    break;
                case ExpressionOperator.GreaterThan:
                    break;
                case ExpressionOperator.GreaterThanOrEqual:
                    break;
                case ExpressionOperator.LessThan:
                    break;
                case ExpressionOperator.LessThanOrEqual:
                    break;
                case ExpressionOperator.All:
                    break;
                case ExpressionOperator.Any:
                    break;
                case ExpressionOperator.Case:
                    break;
                case ExpressionOperator.Coalesce:
                    break;
                case ExpressionOperator.Match:
                    break;
                case ExpressionOperator.InterpolateHcl:
                    break;
                case ExpressionOperator.InterpolateLab:
                    break;
                case ExpressionOperator.Step:
                    break;
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
            }

            throw new NotSupportedException();
        }
    }

    public class ZoomExpression : NumericExpression
    {
        public override double Evaluate(double zoom)
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

    public class NumericInterpolateExpression : InterpolateExpression<double>
    {
        public NumericInterpolateExpression(JArray array) : base(array)
        {
        }

        public override double Evaluate(double zoom)
        {
            throw new NotImplementedException();
        }


        protected override Expression<double> GetStopExpression(JToken token)
        {
            if (token is JArray array)
            {
                var factory = new NumericExpressionFactory();
                return factory.GetExpression(array);
            }

            var value = token.ToObject<double>();
            return new ConstantNumericExpression(value);
        }
    }

    public class InterpolationStop<T>
    {
        public InterpolationStop(double offset, Expression<T> expression)
        {
            Offset = offset;
            Expression = expression;
        }

        public double Offset { get; }

        public Expression<T> Expression { get; }
    }

    public class ConstantNumericExpression : NumericExpression
    {
        public ConstantNumericExpression(double value)
        {
            Value = value;
        }

        public double Value { get; }

        public override double Evaluate(double zoom)
        {
            return Value;
        }
    }

    public abstract class InterpolateExpression<T> : Expression<T>
    {
        public EasingFunction EasingFunction { get; }

        public Expression<double> InputExpression { get; }

        public List<InterpolationStop<T>> Stops { get; } = new List<InterpolationStop<T>>();

        protected InterpolateExpression(JArray array)
        {
            var interpolationFunctionValues = (JArray) array.First;
            var easingMode = interpolationFunctionValues.First.ToObject<EasingMode>();
            var easingModeParameters = interpolationFunctionValues.SkipInto();

            var input = (JArray) array.First.Next;
            
            var inputExpressionFactory = new NumericExpressionFactory();
            InputExpression = inputExpressionFactory.GetExpression(input);

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

            for (var index = 0; index < stopArray.Count; index++)
            {
                var stopOffset = stopArray[index * 2].ToObject<double>();
                var stopValueToken = stopArray[index * 2 + 1];

                Expression<T> stopExpression = GetStopExpression(stopValueToken);

                Stops.Add(new InterpolationStop<T>(stopOffset, stopExpression));
            }
        }

        protected abstract Expression<T> GetStopExpression(JToken token);
    }
}
