# Graphnode.BlazorIconify

**Unofficial, experimental Iconify support for Blazor**  
Uses source generators to embed icon data at build time for fast, offline-friendly rendering.

## Features

- `<Iconify>` component for easy use in Blazor apps
- Icons are embedded in your app via source generators (no runtime fetches for static icon names)
- Falls back to web requests for dynamic icon names
- No JavaScript interop required

## Installation

Install the package Graphnode.BlazorIconify from NuGet.

Add the service in your `Program.cs`:

```csharp
using Graphnode.BlazorIconify;

builder.Services.AddBlazorIconify();
```
*Note:* If it can't resolve `AddBlazorIconify` it's because the source generator did not run.

You can configure the service with options (shown with default values):

```csharp
builder.Services.AddBlazorIconify(options =>
{
    // Allow remote fetching for icons not embedded at build time
    options.EnableRemoteFetching = true;
    
    // Set the remote API URL for fetching icons
    options.RemoteApiUrl = "https://api.iconify.design";
    
    // Throw an exception if the icon is not found in the source cache and remote fetching fails or is disabled.
    options.ThrowIfIconNotFound = false;
});
```

Then add the import in your `_Imports.razor` or in the component where you want to use it:

```razor
@using Graphnode.BlazorIconify
```

## Usage

Use the `<Iconify>` component in your Blazor app to render icons. You can specify the icon name using the `Name` parameter.

```razor
<Iconify Name="mdi:home" />
```

This will render the icon as a `<svg>` element. The icon name should be in the format `prefix:name`, where `prefix` is the icon set (e.g., `mdi` for Material Design Icons) and `name` is the specific icon name.

Any attributes you pass to the component will be passed to the `<svg>` element. For example, you can set the `class` and `style` attributes like this:
```razor
<Iconify Name="mdi:home" class="my-icon" style="width: 24px; height: 24px;" />
```

For dynamic icon names, web fetches will be used as a fallback:
```razor
<Iconify Name="@iconName" />
```

## Notes
- This is unofficial and not affiliated with the Iconify project.
- It's experimental, behavior may change and might break.

## License
This project is licensed under the MIT License.
