namespace AgentForge.Web.Components.Modal;

public sealed class ModalViewModel
{
    public string Id { get; init; } = "";
    public string Title { get; init; } = "";
    public string Description { get; init; } = "";
    public string BodyText { get; init; } = "";
    public string Variant { get; init; } = "standard"; // standard | confirm
    public string PrimaryText { get; init; } = "Confirm";
    public string CancelText { get; init; } = "Cancel";
    public string? PrimaryButtonId { get; init; }
}
