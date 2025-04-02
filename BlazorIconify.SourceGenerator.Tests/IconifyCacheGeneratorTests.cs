using System.Collections.Immutable;
using BlazorIconify.SourceGenerator.Tests.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Graphnode.BlazorIconify.SourceGenerator.Tests;

public class IconifyCacheGeneratorTests
{
    [Fact(DisplayName = "Basic Icons")]
    public void BasicIcons()
    {
        // Create mock Razor files with icon references
        var mockRazorFile1 = new MockAdditionalText("TestComponent1.razor",
            "<div><Iconify Name=\"mdi:home\" /></div>");
        var mockRazorFile2 = new MockAdditionalText("TestComponent2.razor",
            "<div><Iconify Name=\"mdi:account\" /><Iconify Name=\"mdi:home\" /></div>");

        // Run the generator
        var result = RunIconCacheGenerator([mockRazorFile1, mockRazorFile2]);

        // Verify the output
        var generatedCode = GetGeneratedIconRegistry(result);
        foreach (var iconName in new[] { "mdi:home", "mdi:account" })
        {
            Assert.Contains($"case \"{iconName}\":", generatedCode);
        }
    }

    [Fact(DisplayName = "Missing Prefix on API")]
    public void MissingPrefixOnAPI()
    {
        // Create mock Razor files with icon references
        var mockRazorFile1 = new MockAdditionalText("TestComponent1.razor",
            "<div><Iconify Name=\"fake:fake\" /></div>");

        // Run the generator
        var result = RunIconCacheGenerator([mockRazorFile1]);

        // Verify the output
        var generatedCode = GetGeneratedIconRegistry(result);
        Assert.DoesNotContain("case \"fake:fake\":", generatedCode);
    }

    [Fact(DisplayName = "Missing Icon on API")]
    public void MissingIconOnAPI()
    {
        // Create mock Razor files with icon references
        var mockRazorFile1 = new MockAdditionalText("TestComponent1.razor",
            "<div><Iconify Name=\"mdi:fake\" /></div>");

        // Run the generator
        var result = RunIconCacheGenerator([mockRazorFile1]);

        // Verify the output
        var generatedCode = GetGeneratedIconRegistry(result);
        Assert.DoesNotContain("case \"mdi:fake\":", generatedCode);
    }

    [Fact(DisplayName = "No Icons Found")]
    public void NoIconsFound()
    {
        // Create mock Razor files with icon references
        var mockRazorFile1 = new MockAdditionalText("TestComponent1.razor",
            "<div><span>Hello World!</span></div>");

        // Run the generator
        var result = RunIconCacheGenerator([mockRazorFile1]);

        // Verify the output
        var generatedCode = GetGeneratedIconRegistry(result);
        Assert.NotEmpty(generatedCode);
    }

    [Fact(DisplayName = "Icon with no Name")]
    public void IconWithNoName()
    {
        // Create mock Razor files with icon references
        var mockRazorFile1 = new MockAdditionalText("TestComponent1.razor",
            "<div><Iconify /></div>");

        // Run the generator
        var result = RunIconCacheGenerator([mockRazorFile1]);

        // Verify the output
        var generatedCode = GetGeneratedIconRegistry(result);
        Assert.NotEmpty(generatedCode);
    }

    // Utility method to run the generator with given additional texts
    private static GeneratorDriverRunResult RunIconCacheGenerator(ImmutableArray<AdditionalText> additionalTexts)
    {
        // Create the generator instance
        var generator = new IconifyCacheGenerator();

        // Create a minimal compilation
        var compilation = CSharpCompilation.Create(
            "TestCompilation",
            Array.Empty<SyntaxTree>(),
            [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)]
        );

        // Configure and run the generator
        var driver = CSharpGeneratorDriver.Create(generator)
            .AddAdditionalTexts(additionalTexts);

        driver = driver.RunGenerators(compilation);

        var runResult = driver.GetRunResult();
        return runResult;
    }

    private static string GetGeneratedIconRegistry(GeneratorDriverRunResult result)
    {
        var generatedFile = result.GeneratedTrees.FirstOrDefault(
            t => t.FilePath.EndsWith("IconifyCache.g.cs"));

        Assert.NotNull(generatedFile);

        var generatedCode = generatedFile.GetText().ToString();

        return generatedCode;
    }
}
