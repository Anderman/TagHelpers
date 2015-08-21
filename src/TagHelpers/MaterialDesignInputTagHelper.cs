using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;

namespace Anderman.TagHelpers
{
    [TargetElement("input", Attributes = "asp-material-design")]
    public class MetrialDesignTagHelper : TagHelper
    {
        [HtmlAttributeName("asp-for")]
        public ModelExpression For { get; set; }
        [HtmlAttributeName("asp-material-design")]
        public bool MaterialDesign { get; set; }

        [HtmlAttributeName("type")]
        public string InputTypeName { get; set; }
        /// <summary>
        /// <div class="form-group form-md-line-input">
        ///    <label class="col-md-2 control-label" asp-for="AccessFailedCount">Regular input</label>
        ///    <div class="col-md-10">
        ///        <input asp-for="AccessFailedCount" class="form-control"/>
        ///        <div class="form-control-focus"></div>
        ///    </div>
        ///</div>
        /// </summary>
        /// <param name="context"></param>
        /// <param name="output"></param>
        /// <returns></returns>

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var description = For.Metadata.Description;
            var displayname = For.Metadata.GetDisplayName();
            var placeholder = For.Metadata.GetPlaceholder();

            await context.GetChildContentAsync();
            //output.PreElement.Append("<div class='input-field col s6'>");
            if (!output.Content.GetContent().Contains("checkbox"))
            {
                output.PreElement.Append("<div class='form-control-wrapper'>");
                if (For.Model == null)
                    if (output.Attributes.ContainsName("class"))
                        output.Attributes["class"].Value += " empty";
                    else
                        output.Attributes.Add("class", " empty");
                var active = For.Model != null ? "class='active'" : "";
                output.PostElement.Append($"<div class='floating-label' for='{For.Name}' {active}>{displayname}</div>");
                output.PostElement.Append($"<span class='text-danger field-validation-valid' data-valmsg-for='{For.Name}' data-valmsg-replace='true'></span>");
                output.PostElement.Append(@"</div>");//close row and field
            }
            else
            {
                //< div class="checkbox">
                //  <label>
                //    <input type = "checkbox" checked="">
                //    Auto -updates
                //    </label>
                //</div>
                var togglebutton = false;
                if (context.AllAttributes.ContainsName("class") && context.AllAttributes["class"].Value.ToString().Contains("togglebutton"))
                    togglebutton = true;

                output.PreElement.Append(@"<div class='" + (togglebutton ? "togglebutton" : "checkbox") + "'>");
                output.PreElement.Append(@"<label>");
                output.Content.SetContent(output.Content.GetContent().Replace("/><input", "/><span class='"+ (togglebutton ? "toggle" : "checkbox-material") + "'><span class=check></span></span><input"));
                output.PostElement.Append(" " + displayname);
                //
                output.PostElement.Append($"<span class='text-danger field-validation-valid' data-valmsg-for='{For.Name}' data-valmsg-replace='true'></span>");
                //
                output.PostElement.Append(@"</label>");
                output.PostElement.Append(@"</div>");

                //output.Content.SetContent(output.Content.GetContent().Replace("form-control", "md-check"));
                //output.PostElement.Append($"<label for='{For.Name}'>{displayname}</label>");
                //output.PreElement.Append($"<label class='control-label' for='{For.Name}'></label>");
                //output.PreElement.Append(@"<div class='md-checkbox'>");
                //output.PostElement.Append($"<label for='{For.Name}'>{displayname}");
                //output.PostElement.Append(@"<span></span><span class='check'></span><span class='box'></span></label></div>");
            }
            //output.PostElement.Append(@"<div class='form-control-focus'></div>");
            //output.PostElement.Append($"<span class='help-block'>{description}</span></div>");
            return;
        }
    }
}