using AgentForge.Web.Components.Button;
using AgentForge.Web.Components.Delete;
using AgentForge.Web.Components.Modal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace AgentForge.Web.TagHelpers;

// Single Razor-facing API for the shared design-system components.
// Existing component views remain the sole markup owners during migration.
[HtmlTargetElement("ui-button")]
[HtmlTargetElement("ui-input")]
[HtmlTargetElement("ui-select")]
[HtmlTargetElement("ui-textarea")]
[HtmlTargetElement("ui-field")]
[HtmlTargetElement("ui-badge")]
[HtmlTargetElement("ui-card")]
[HtmlTargetElement("ui-modal")]
[HtmlTargetElement("ui-delete")]
public sealed class UiComponentTagHelper : TagHelper
{
    private readonly IHtmlHelper _html;
    private readonly IViewComponentHelper _components;

    public UiComponentTagHelper(IHtmlHelper html, IViewComponentHelper components)
    {
        _html = html;
        _components = components;
    }

    [ViewContext, HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; } = default!;

    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Text { get; set; }
    public string? Variant { get; set; }
    public string? Type { get; set; }
    public string? AutoComplete { get; set; }
    public string? CssClass { get; set; }
    public string? AriaLabel { get; set; }
    public string? AriaHasPopup { get; set; }
    public string? AriaControls { get; set; }
    public string? Label { get; set; }
    public string? Hint { get; set; }
    public string? Placeholder { get; set; }
    public string? Title { get; set; }
    public string? TitleId { get; set; }
    public string? DescriptionId { get; set; }
    public string? Description { get; set; }
    public string? BodyText { get; set; }
    public string? PrimaryText { get; set; }
    public string? PrimaryButtonId { get; set; }
    public string? CloseButtonId { get; set; }
    public string? CancelButtonId { get; set; }
    public string? PreviousButtonId { get; set; }
    public string? NextButtonId { get; set; }
    public string? SubmitButtonId { get; set; }
    public string? CancelText { get; set; }
    public string? ConfirmText { get; set; }
    public string? ConfirmButtonId { get; set; }
    public string? ItemName { get; set; }
    public string? Control { get; set; }
    public int? MaxLength { get; set; }
    public int? Rows { get; set; }
    public bool Required { get; set; }
    public bool Disabled { get; set; }
    public bool Hidden { get; set; }
    public bool CustomLayout { get; set; }
    public IEnumerable<SelectListItem>? Options { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = null;
        if (_html is IViewContextAware htmlAware) htmlAware.Contextualize(ViewContext);
        if (_components is IViewContextAware componentAware) componentAware.Contextualize(ViewContext);

        var tag = context.TagName;
        if (tag == "ui-button")
        {
            var child = await output.GetChildContentAsync();
            // Child content is allowed for the common <ui-button>Save</ui-button> form.
            var label = Text ?? child.GetContent();
            var rendered = await _components.InvokeAsync("Button", new
            {
                model = new ButtonViewModel
                {
                    Text = label, Id = Id, Variant = Variant ?? "secondary",
                    Type = Type ?? "button", CssClass = CssClass,
                    AriaLabel = AriaLabel, AriaHasPopup = AriaHasPopup,
                    AriaControls = AriaControls, Disabled = Disabled, Hidden = Hidden
                }
            });
            output.Content.SetHtmlContent(rendered);
            return;
        }

        if (tag is "ui-modal" or "ui-delete")
        {
            var customContent = tag == "ui-modal" ? await output.GetChildContentAsync() : null;
            var rendered = tag == "ui-modal"
                ? await _components.InvokeAsync("Modal", new
                {
                    model = new ModalViewModel
                    {
                        Id = Id ?? "", Title = Title ?? "", Description = Description ?? "",
                        TitleId = TitleId, DescriptionId = DescriptionId,
                        CloseButtonId = CloseButtonId, CancelButtonId = CancelButtonId,
                        PreviousButtonId = PreviousButtonId, NextButtonId = NextButtonId, SubmitButtonId = SubmitButtonId,
                        CssClass = CssClass, CustomLayout = CustomLayout, CustomContent = customContent is not null && !customContent.IsEmptyOrWhiteSpace,
                        Content = customContent,
                        BodyText = BodyText ?? "", Variant = Variant ?? "standard",
                        PrimaryText = PrimaryText ?? "Confirm", CancelText = CancelText ?? "Cancel",
                        PrimaryButtonId = PrimaryButtonId
                    }
                })
                : await _components.InvokeAsync("Delete", new
                {
                    model = new DeleteViewModel
                    {
                        Id = Id ?? "", Title = Title ?? "Delete item?",
                        Description = Description ?? "This action cannot be undone.",
                        ItemName = ItemName ?? "", ConfirmButtonId = ConfirmButtonId ?? "",
                        ConfirmText = ConfirmText ?? "Delete", CancelText = CancelText ?? "Cancel"
                    }
                });
            output.Content.SetHtmlContent(rendered);
            return;
        }

        var view = tag switch
        {
            "ui-input" => "Input", "ui-select" => "Select",
            "ui-textarea" => "Textarea", "ui-field" => "Field",
            "ui-badge" => "Badge", "ui-card" => "Card",
            _ => throw new InvalidOperationException($"Unknown UI component: {tag}")
        };
        var data = new ViewDataDictionary(ViewContext.ViewData)
        {
            ["Id"] = Id, ["Name"] = Name, ["Text"] = Text,
            ["Type"] = Type, ["AutoComplete"] = AutoComplete,
            ["Variant"] = Variant, ["Label"] = Label, ["Hint"] = Hint,
            ["Placeholder"] = Placeholder, ["Title"] = Title,
            ["Description"] = Description, ["Control"] = Control,
            ["Required"] = Required, ["MaxLength"] = MaxLength,
            ["Rows"] = Rows, ["Options"] = Options
        };
        var partial = await _html.PartialAsync($"~/Components/{view}/Default.cshtml", null, data);
        output.Content.SetHtmlContent(partial);
    }
}
