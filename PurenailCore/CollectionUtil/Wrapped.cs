namespace PurenailCore.CollectionUtil;

// Basic wrapper class, mainly intended for Lambda captures of value types.
public class Wrapped<T>(T value)
{
    public T Value = value;
}
