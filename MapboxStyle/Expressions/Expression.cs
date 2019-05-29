using System;

namespace MapboxStyle.Expressions
{
    public class Expression<T> : IExpression<T>
    {
        public T GetValue(double zoom)
        {
            throw new NotImplementedException();
        }
    }
}
