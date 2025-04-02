namespace Graphnode.BlazorIconify;

public sealed class BlazorIconifyOptions
{
    /// <summary>
    /// Whether to enable fetching icons from the remote API when they are not found in the source cache.
    /// </summary>
    public bool EnableRemoteFetching { get; set; } = true;

    /// <summary>
    /// The URL of the remote API to fetch icons from.
    /// </summary>
    public string RemoteApiUrl { get; set; } = "https://api.iconify.design";

    /// <summary>
    /// Throw an exception if the icon is not found in the source cache and remote fetching fails or is disabled.
    /// </summary>
    public bool ThrowIfIconNotFound { get; set; } = false;
}
