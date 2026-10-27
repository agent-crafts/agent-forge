namespace AgentForge.Web.Components.Modal;

public sealed class ModalViewModel
{
    public Microsoft.AspNetCore.Html.IHtmlContent? Content { get; init; }
    public bool CustomContent { get; init; }
    public bool CustomLayout { get; init; }
    public string? CssClass { get; init; }
    public string? TitleId { get; init; }
    public string? DescriptionId { get; init; }
    public string? CloseButtonId { get; init; }
    public string? CancelButtonId { get; init; }
    public string Id { get; init; } = "";
    public string Title { get; init; } = "";
    public string Description { get; init; } = "";
    public string BodyText { get; init; } = "";
    public string Variant { get; init; } = "standard"; // standard | confirm
    public string PrimaryText { get; init; } = "Confirm";
    public string CancelText { get; init; } = "Cancel";
    public string? PrimaryButtonId { get; init; }
}
