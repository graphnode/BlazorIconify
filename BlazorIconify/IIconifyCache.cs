namespace Graphnode.BlazorIconify;

public interface IIconifyCache
{
    (string Prefix, string Icon, string? Body, int Width, int Height) GetIcon(string name);
}
