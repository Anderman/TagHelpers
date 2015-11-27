using System;
using Microsoft.AspNet.Razor.TagHelpers;

namespace Anderman.Taghelpers
{
    [HtmlTargetElement(Attributes= ForEmptyAttributeName)]
    public class ForEmptyTagHelper : TagHelper
    {
        private const string ForEmptyAttributeName = "asp-for-empty";
        /// <summary>
        /// The ForElse part is shown if forloop has no content
        /// </summary>
        //[HtmlAttributeName(ForEmptyAttributeName)]
        //public string ForEmpty { get; set; }
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var guid = Guid.NewGuid();
            output.Attributes.Add(new TagHelperAttribute("id", guid));
            output.Attributes.Add(new TagHelperAttribute("style", "display:none;"));
            output.PostContent.AppendHtml($@"<script>
                    var elseEl=document.getElementById('{guid}');
                    var prev1=elseEl.previousSibling;
                    var prev2=prev1?prev1.previousSibling:prev1;
                    var forEl=prev1.tagName?prev1:prev2;
                    if(!(forEl.innerHTML && forEl.innerHTML.replace(/^\s+|\s+$/g,'').length>0))
                        elseEl.removeAttribute('style');
                </script>");
        }
    }
}