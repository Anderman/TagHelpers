using System.Diagnostics;
using System.Threading.Tasks;
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

        [HtmlAttributeName("div-class")]
        public string DivClass { get; set; }

        [HtmlAttributeName("type")]
        public string InputTypeName { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var description = For.Metadata.Description;
            var displayname = For.Metadata.GetDisplayName();
            var placeholder = For.Metadata.GetPlaceholder();

            await output.GetChildContentAsync();

            //AddEmptyClassIfValueIsEmpty(context, output);
            if (!output.Content.GetContent().Contains("checkbox"))
            {
                if (DivClass != null) output.Attributes.Remove("div-class");
                output.PreElement.AppendHtml($"\n<div class='form-group {DivClass} label-floating'>" +
                    $"\n   <label class='control-label' for='{For.Name}'>{displayname}</label>\n   ");
                //var active = For.Model != null ? "active" : "";
                //<label class="control-label" for="focusedInput2">Focus to show the help-block</label>
                //output.PostElement.AppendHtml($"<label class='control-label' for='{For.Name}'>{displayname}</label>");

                output.PostElement.AppendHtml($"\n   <span class='text-danger field-validation-valid' data-valmsg-for='{For.Name}' data-valmsg-replace='true'></span>");
                output.PostElement.AppendHtml("\n</div>");//close row and field
            }
            else
            {
                //<div class="checkbox"><label><input type="checkbox">name</label>
                 var togglebutton = hasClass(context, "togglebutton");
                output.PreElement.AppendHtml(@"<div class='" + (togglebutton ? "togglebutton" : "checkbox") + "'>");
                output.PreElement.AppendHtml(@"<label>");
                output.PostElement.AppendHtml(" " + displayname);
                output.PostElement.AppendHtml($"<span class='text-danger field-validation-valid' data-valmsg-for='{For.Name}' data-valmsg-replace='true'></span>");
                output.PostElement.AppendHtml(@"</label>");
                output.PostElement.AppendHtml(@"</div>");
            }
            return;
        }
        private void AddEmptyClassIfValueIsEmpty(TagHelperContext context, TagHelperOutput output)
        {
            IReadOnlyTagHelperAttribute value = null;
            context.AllAttributes.TryGetAttribute("value", out value);
            if (For.Model == null && value?.Value == null)
                if (output.Attributes.ContainsName("class"))
                    output.Attributes["class"].Value += " empty";
                else
                    output.Attributes.Add("class", " empty");
        }
        private bool hasClass(TagHelperContext context, string value)
        {
            return context.AllAttributes.ContainsName("class") && context.AllAttributes["class"].Value.ToString().Contains(value);
        }
    }
}