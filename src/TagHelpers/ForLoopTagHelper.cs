using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Anderman.Taghelpers
{
    [HtmlTargetElement(Attributes =ForLoopAttributeName)]
    public class ForLoopTagHelper : TagHelper
    {
        private const string ForLoopAttributeName = "asp-for-loop";
        /// <summary>
        /// if the content of this Tag is empty then the Tag with the forelse attribute is shown
        /// </summary>
        //[HtmlAttributeName(ForLoopAttributeName)]
        //public string ForLoop { get; set; }
    }
}