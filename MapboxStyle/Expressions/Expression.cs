using System.Collections.Generic;

namespace MapboxStyle.Expressions
{
    public abstract class Expression<T> : IExpression<T>
    {
        public abstract T Evaluate(FilterType featureType, string featureId, double zoom,
            IDictionary<string, string> featureProperties);
    }

    public abstract class NumericExpression : Expression<double>
    {
    }
}
