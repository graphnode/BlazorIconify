namespace Graphnode.BlazorIconify;

public class BlazorIconifyException : Exception
{
    public BlazorIconifyException(string message) : base(message) { }

    public BlazorIconifyException(string message, Exception innerException) : base(message, innerException) { }
}
