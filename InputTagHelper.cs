using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;

namespace DBC.Helpers
{
    [TargetElement("input", Attributes = "asp-for")]
    public class InputTagHelper : TagHelper
    {
        [HtmlAttributeName("asp-for")]
        public ModelExpression For { get; set; }

        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (!context.AllAttributes.ContainsKey("placeholder"))
                output.Attributes.Add("placeholder", For?.Metadata?.DisplayName);
            return Task.FromResult(0);
        }
    }
}