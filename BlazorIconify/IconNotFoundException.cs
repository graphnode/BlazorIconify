namespace Graphnode.BlazorIconify;

public class IconNotFoundException : Exception
{
    public IconNotFoundException(string message) : base(message) { }

    public IconNotFoundException(string message, Exception innerException) : base(message, innerException) { }
}
