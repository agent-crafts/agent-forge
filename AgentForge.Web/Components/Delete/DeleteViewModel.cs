namespace AgentForge.Web.Components.Delete;

public sealed class DeleteViewModel
{
    public string Id { get; init; } = "";
    public string Title { get; init; } = "Delete item?";
    public string Description { get; init; } = "This action cannot be undone.";
    public string ItemName { get; init; } = "";
    public string ConfirmButtonId { get; init; } = "";
    public string ConfirmText { get; init; } = "Delete";
    public string CancelText { get; init; } = "Cancel";
}
