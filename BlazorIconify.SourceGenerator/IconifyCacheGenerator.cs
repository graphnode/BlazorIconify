using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Graphnode.BlazorIconify.SourceGenerator;

[Generator]
public class IconifyCacheGenerator : IIncrementalGenerator
{
    private static readonly HttpClient _httpClient = new();

    private static readonly MemoryCache _cache = new();

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Find all additional files that end with .razor
        var razorFiles = context.AdditionalTextsProvider
            .Where(static file => file.Path.EndsWith(".razor", StringComparison.OrdinalIgnoreCase));

        // Extract all icon names from the razor files
        var iconNames = razorFiles.SelectMany(static (text, ct)
            => ExtractIconNames(text.GetText(ct)!.ToString()));

        // Flatten the icon names and remove duplicates
        var distinctIconNames = iconNames
            .Collect()
            .Select(static (names, _) => names.Distinct());

        // Download source for each icon in groups
        var iconsToGenerate = distinctIconNames.SelectMany(DownloadIconSources);

        // Register the source output
        context.RegisterSourceOutput(iconsToGenerate.Collect(), GenerateIconRegistry);

        context.RegisterPostInitializationOutput(initializationContext =>
        {
            initializationContext.AddSource("IconifyConfig.g.cs", SourceText.From("""
                #nullable enable
                
                using System;
                using Microsoft.Extensions.DependencyInjection;

                namespace Graphnode.BlazorIconify;

                public static class IconifyConfig
                {
                    public static IServiceCollection AddBlazorIconify(this IServiceCollection services, Action<BlazorIconifyOptions>? configure = null)
                    {
                        services.AddSingleton<IIconifyCache, IconifyCache>();
                        
                        if (configure != null)
                        {
                            services.Configure(configure);
                        }
                
                        return services;
                    }
                }

                """, Encoding.UTF8));
        });
    }

    private static IEnumerable<string> ExtractIconNames(string razorContent)
    {
        var matches = Regex.Matches(razorContent, @"<Iconify\s+[^>]*Name\s*=\s*""([^""]+)""");
        foreach (Match match in matches)
        {
            yield return match.Groups[1].Value;
        }
    }

    private static IEnumerable<IconData> DownloadIconSources(IEnumerable<string> iconNames, CancellationToken ct)
    {
        // Group icon names by prefix
        var iconsByPrefix = iconNames
            .Select(fullName =>
            {
                var parts = fullName.Split(':');
                if (parts.Length != 2) return null;
                return new { FullName = fullName, Prefix = parts[0], IconName = parts[1] };
            })
            .Where(item => item != null)
            .GroupBy(item => item!.Prefix);

        // For each prefix, download all icons in one request
        foreach (var prefixGroup in iconsByPrefix)
        {
            var prefix = prefixGroup.Key;
            //var icons = string.Join(",", prefixGroup.Select(item => item!.IconName));
            var icons = prefixGroup.Select(item => item!.IconName);

            foreach (var icon in icons)
            {
                if (!_cache.TryGetValue($"{prefix}:{icon}", out string? jsonString) || string.IsNullOrWhiteSpace(jsonString))
                {
                    var url = $"https://api.iconify.design/{prefix}.json?icons={icon}";
                    var response = _httpClient.GetAsync(url, ct).Result;

                    if (!response.IsSuccessStatusCode)
                        continue;

                    jsonString = response.Content.ReadAsStringAsync().Result;

                    if (string.IsNullOrWhiteSpace(jsonString) || int.TryParse(jsonString, out _))
                        continue;

                    _cache.Set($"{prefix}:{icon}", jsonString, TimeSpan.FromHours(1));
                }

                if (string.IsNullOrWhiteSpace(jsonString))
                    continue;

                var iconifyJson = JsonNode.Parse(jsonString!);
                if (iconifyJson == null)
                    continue;

                // Get width and height from the JSON
                var defaultWidth = iconifyJson["width"]?.GetValue<int?>() ?? 16;
                var defaultHeight = iconifyJson["height"]?.GetValue<int?>() ?? 16;

                // Access the icons object and iterate through its properties
                var iconsObject = iconifyJson["icons"]?.AsObject();
                if (iconsObject == null)
                    continue;

                foreach(var iconProperty in iconsObject)
                {
                    var iconName = iconProperty.Key;
                    var value = iconProperty.Value;
                    var iconBody = value?["body"]?.GetValue<string?>();
                    if (value == null || iconBody == null)
                        continue;

                    var width = value["width"]?.GetValue<int?>() ?? defaultWidth;
                    var height = value["height"]?.GetValue<int?>() ?? defaultHeight;

                    yield return new IconData(prefix, iconName, iconBody, width, height);
                }
            }
        }
    }

    private static void GenerateIconRegistry(SourceProductionContext context, ImmutableArray<IconData> icons)
    {
        var sb = new StringBuilder();
        sb.AppendLine("""
            #nullable enable
            
            namespace Graphnode.BlazorIconify;

            public class IconifyCache : IIconifyCache
            {
                public IconData GetIcon(string name)
                {
                    switch (name)
                    {
            """);

        foreach (var icon in icons)
        {
            sb.AppendLine($"""
                            case "{icon.Prefix}:{icon.Name}":
                                return new IconData("{icon.Prefix}", "{icon.Name}", "{icon.Body.Replace("\"", "\\\"")}", {icon.Width}, {icon.Height});
                """);
        }

        sb.AppendLine("""
                        default:
                            return default;
                    }
                }
            }
            """);

        context.AddSource("IconifyCache.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
    }
}
