using Markdig;
using Microsoft.AspNetCore.Components;

namespace Dilcore.WebApp.Services.Agent;

internal sealed class MarkdownRenderer : IMarkdownRenderer
{
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .DisableHtml()
        .Build();

    public MarkupString ToHtml(string? markdown)
    {
        if (string.IsNullOrEmpty(markdown))
        {
            return new MarkupString(string.Empty);
        }

        var html = Markdown.ToHtml(markdown, Pipeline);
        return new MarkupString(html);
    }
}
