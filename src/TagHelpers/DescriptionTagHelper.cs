using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Anderman.TagHelpers
{
    [HtmlTargetElement("description", Attributes = "asp-for")]
    public class DescriptionTagHelper : TagHelper
    {
        [HtmlAttributeName("asp-for")]
        public ModelExpression For { get; set; }

        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (!string.IsNullOrEmpty(For?.Metadata.Description))
            {
                output.TagName = "SPAN";
                output.Content.AppendHtml(For?.Metadata.Description);
            }
            else
                output.TagName = "";

            return Task.FromResult(0);
        }
    }
}