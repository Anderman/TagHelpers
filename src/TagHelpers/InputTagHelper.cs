using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Razor.TagHelpers;
using System.Linq;

namespace Anderman.TagHelpers
{
    [HtmlTargetElement("input", Attributes = "asp-for")]
    public class InputTagHelper : TagHelper
    {
        [HtmlAttributeName("asp-for")]
        public ModelExpression For { get; set; }

        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (!context.AllAttributes.ContainsName("placeholder") && For?.Metadata.GetPrompt() != null)
                output.Attributes.Add("placeholder", For?.Metadata.GetPrompt());
           
            return Task.FromResult(0);
        }
    }
}