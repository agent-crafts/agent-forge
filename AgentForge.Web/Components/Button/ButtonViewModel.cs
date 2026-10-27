namespace AgentForge.Web.Components.Button;
public sealed class ButtonViewModel
{
    public string Text { get; init; } = "";
    public string Variant { get; init; } = "secondary";
    public string Type { get; init; } = "button";
    public string? Id { get; init; }
    public bool Disabled { get; init; }
    public string? AriaLabel { get; init; }
    public string? CssClass { get; init; }
    public bool Hidden { get; init; }
}
