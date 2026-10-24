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
    public bool CloseDialog { get; init; }
    public bool Disabled { get; init; }
    public string? Role { get; init; }
    public string? AriaControls { get; init; }
    public string? AriaSelected { get; init; }
    public int? TabIndex { get; init; }
    public string? CopyTarget { get; init; }
    public string? Icon { get; init; }
    public string? LabelId { get; init; }
}
