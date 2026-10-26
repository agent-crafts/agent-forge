namespace AgentForge.Web.Features.Shared.Components.Button;
public sealed class ButtonViewModel
{
    public string Text { get; init; } = "";
    public string Variant { get; init; } = "secondary";
    public string Type { get; init; } = "button";
    public string? Id { get; init; }
    public bool Disabled { get; init; }
    public bool Hidden { get; init; }
}
