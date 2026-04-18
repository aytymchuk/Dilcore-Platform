using Microsoft.AspNetCore.Components;

namespace Dilcore.WebApp.Services.Agent;

public interface IMarkdownRenderer
{
    MarkupString ToHtml(string? markdown);
}
