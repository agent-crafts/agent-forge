namespace AgentForge.Web.Models;

public enum UiButtonVariant
{
    Primary,
    Secondary,
    Cancel,
    Outlined,
    Danger,
    Ghost,
    Success
}

public sealed class UiButtonModel
{
    public required string Text { get; init; }
    public UiButtonVariant Variant { get; init; } = UiButtonVariant.Secondary;
    public string Type { get; init; } = "button";
    public string? Id { get; init; }
    public string? CssClass { get; init; }
    public string? DataAttribute { get; init; }
    public bool Disabled { get; init; }
}
