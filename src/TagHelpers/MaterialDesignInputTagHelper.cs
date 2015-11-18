﻿using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Razor.TagHelpers;

namespace Anderman.TagHelpers
{
    [HtmlTargetElement("input", Attributes = "asp-material-design")]
    public class MetrialDesignTagHelper : TagHelper
    {
        [HtmlAttributeName("asp-for")]
        public ModelExpression For { get; set; }
        [HtmlAttributeName("asp-material-design")]
        public bool MaterialDesign { get; set; }

        [HtmlAttributeName("type")]
        public string InputTypeName { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var description = For.Metadata.Description;
            var displayname = For.Metadata.GetDisplayName();
            var placeholder = For.Metadata.GetPlaceholder();

            await output.GetChildContentAsync();
            IReadOnlyTagHelperAttribute value = null;
            context.AllAttributes.TryGetAttribute("value", out value);
            if (!output.Content.GetContent().Contains("checkbox")) {
                output.PreElement.AppendHtml("<div class='form-control-wrapper'>");
                if (For.Model == null && value?.Value == null)
                    if (output.Attributes.ContainsName("class"))
                        output.Attributes["class"].Value += " empty";
                    else
                        output.Attributes.Add("class", " empty");
                var active = For.Model != null ? "class='active'" : "";
                output.PostElement.AppendHtml($"<div class='floating-label' for='{For.Name}' {active}>{displayname}</div>");
                output.PostElement.AppendHtml($"<span class='text-danger field-validation-valid' data-valmsg-for='{For.Name}' data-valmsg-replace='true'></span>");
                output.PostElement.AppendHtml(@"</div>");//close row and field
            }
            else {
                var togglebutton = false;
                if (context.AllAttributes.ContainsName("class") && context.AllAttributes["class"].Value.ToString().Contains("togglebutton"))
                    togglebutton = true;

                output.PreElement.AppendHtml(@"<div class='" + (togglebutton ? "togglebutton" : "checkbox") + "'>");
                output.PreElement.AppendHtml(@"<label>");
                output.Content.SetHtmlContent(output.Content.GetContent().Replace("/><input", "/><span class='" + (togglebutton ? "toggle" : "checkbox-material") + "'><span class=check></span></span><input"));
                output.PostElement.AppendHtml(" " + displayname);
                output.PostElement.AppendHtml($"<span class='text-danger field-validation-valid' data-valmsg-for='{For.Name}' data-valmsg-replace='true'></span>");
                output.PostElement.AppendHtml(@"</label>");
                output.PostElement.AppendHtml(@"</div>");
            }
            return;
        }
    }
}