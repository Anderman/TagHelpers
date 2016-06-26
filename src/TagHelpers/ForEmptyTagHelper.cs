using System;
using Microsoft.AspNetCore.Razor.TagHelpers;

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
        //The script is in a file so that html document is a valid xmlDocument for testing
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.Attributes.Add(new TagHelperAttribute("style", "display:none;"));
            var x=output.PostContent.AppendHtml("<script src='/jsnocat/forempty.min.js'></script>");
            //var s=document.getElementsByTagName("script"),e=s[s.length-1].parentElement,p1=e.previousSibling,p2=p1?p1.previousSibling:p1,f=p1.tagName?p1:p2;f.innerHTML&&f.innerHTML.replace(/^\s+|\s+$/g,"").length>0||(e.removeAttribute("style"),f.setAttribute("style","display:none;"));
        }
    }
}