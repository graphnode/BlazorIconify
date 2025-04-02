using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Graphnode.BlazorIconify.Tests.Utilities;

// Helper class to create mock additional text files
public class MockAdditionalText(string path, string text) : AdditionalText
{
    public override string Path { get; } = path;

    public override SourceText GetText(CancellationToken cancellationToken = default)
    {
        return SourceText.From(text, Encoding.UTF8);
    }
}
