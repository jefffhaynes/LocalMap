namespace MapboxStyle
{
    public interface IExpression<T>
    {
        T GetValue(double zoom);
    }
}
