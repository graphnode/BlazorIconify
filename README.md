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
