using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;

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
                output.Content.AppendEncoded(For?.Metadata.Description);
            }
            else
                output.TagName = "";

            return Task.FromResult(0);
        }
    }
}