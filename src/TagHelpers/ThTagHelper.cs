using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;

namespace Anderman.TagHelpers
{
    [TargetElement("th", Attributes = "asp-for")]
    public class thTagHelper : TagHelper
    {
        [HtmlAttributeName("asp-for")]
        public ModelExpression For { get; set; }

        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.Attributes.Add("column-data", For.Name);
            output.Content.Append(For?.Metadata?.DisplayName??For.Name);
            return Task.FromResult(0);
        }
    }
}