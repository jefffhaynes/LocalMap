using System.Linq;
using Newtonsoft.Json.Linq;

namespace MapboxStyle.Expressions
{
    public abstract class Expression<T>
    {
        public abstract T Evaluate(double zoom);
    }

    public abstract class NumericExpression : Expression<double>
    {
        protected NumericExpressionFactory Factory = new NumericExpressionFactory();
    }
}
