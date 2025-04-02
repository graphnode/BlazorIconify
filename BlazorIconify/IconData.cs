namespace Graphnode.BlazorIconify;

public readonly struct IconData(string prefix, string name, string body, int width, int height)
{
    public string Prefix { get; } = prefix;

    public string Name { get; } = name;

    public string Body { get; } = body;

    public int Width { get; } = width;

    public int Height { get; } = height;

    public bool IsValid => !string.IsNullOrWhiteSpace(Body) &&
                           !string.IsNullOrWhiteSpace(Name) &&
                           !string.IsNullOrWhiteSpace(Prefix) &&
                           Width > 0 && Height > 0;
}
