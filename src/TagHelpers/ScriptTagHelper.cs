using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc.TagHelpers;
using Microsoft.AspNet.Razor.TagHelpers;

namespace Anderman.TagHelpers
{
    [HtmlTargetElement("script", Attributes = "asp-fallback-src")]
    [HtmlTargetElement("script", Attributes = "asp-copy-src-to-fallback")]
    [HtmlTargetElement("script", Attributes = "asp-use-minified")]

    public class ScriptTagHelper : TagHelper
    {
        private readonly string _preTest = "<script>({0}==null||alert('{1}'));</script>\n";
        private readonly string _postTest = "\n<script>({0}!=null||alert('{1}'));</script>\n";
        [HtmlAttributeName("asp-copy-src-to-fallback")]
        public string CopySrcToFallback { get; set; }

        [HtmlAttributeName("asp-warn-if-test-is-invalid")]
        public string WarnIfTestIsInvalid { get; set; }

        [HtmlAttributeName("asp-fallback-src")]
        public string FallbackSrc { get; set; }

        [HtmlAttributeName("asp-fallback-test")]
        public string FallbackTest { get; set; }

        [HtmlAttributeName("src")]
        public string RemotePath { get; set; }
        [HtmlAttributeName("asp-use-minified")]
        public string UseMinified { get; set; }

        [HtmlAttributeName("asp-use-site-min-js")]
        public string UseSiteMinJs { get; set; }

        [HtmlAttributeName("asp-use-local")]
        public string UseLocal { get; set; }

        protected internal IHostingEnvironment HostingEnvironment { get; set; }
        protected internal IHttpContextAccessor Context { get; set; }
        public ScriptTagHelper(IHostingEnvironment hostingEnvironment, IHttpContextAccessor context)
        {
            HostingEnvironment = hostingEnvironment;
            Context = context;
        }
        //
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            bool useSiteMinJs = UseSiteMinJs?.Contains(HostingEnvironment.EnvironmentName, StringComparison.OrdinalIgnoreCase) == true;
            if (WarnIfTestIsInvalid?.Contains(HostingEnvironment.EnvironmentName, StringComparison.OrdinalIgnoreCase) == true)
            {
                var x = await output.GetChildContentAsync();
                if (!useSiteMinJs)
                    output.PreElement.AppendHtml(string.Format(_preTest, FallbackTest, $"Script `{RemotePath}` already loaded. Did you create the correct test"));
                output.PostElement.AppendHtml(string.Format(_postTest, FallbackTest, $"Script `{RemotePath}` still not loaded. Did you create the correct test"));
            }
            var LocalRelPath = "";
            if (CopySrcToFallback?.Contains(HostingEnvironment.EnvironmentName, StringComparison.OrdinalIgnoreCase) == true)
            {
                try
                {
                    var scheme = Context.HttpContext.Request.Scheme;
                    RemotePath = RemotePath.StartsWith("//")
                        ? scheme + ":" + RemotePath
                        : RemotePath.ToLower().StartsWith("/") || RemotePath.ToLower().StartsWith("~/")
                        ? scheme + "://" + Context.HttpContext.Request.Host + RemotePath.Replace("~", "")
                        : RemotePath;

                    LocalRelPath = (useSiteMinJs == false) ?
                        "/js/" + new Uri(RemotePath).Segments.Last()
                        : FallbackSrc ?? "/fallback/js/" + new Uri(RemotePath).Segments.Last();
                    var localPath = HostingEnvironment.MapPath(LocalRelPath.TrimStart('/'));
                    var pathHelper = new PathHelper(RemotePath, localPath);
                    if (!File.Exists(localPath))
                    {
                        using (var webClient = new HttpClient())
                        {
                            var file = await webClient.GetStringAsync(new Uri(RemotePath));
                            Directory.CreateDirectory(pathHelper.LocalDirectory);
                            File.WriteAllText(localPath, file);
                            if (RemotePath.Contains(".min.") && UseSiteMinJs != null)//copy full version to let gulp minify this verion to site.css
                            {
                                file = await webClient.GetStringAsync(new Uri(RemotePath.Replace(".min", "")));
                                File.WriteAllText(localPath, file);
                            }
                            if (!RemotePath.Contains(".min.") && UseMinified != null)
                            {
                                file = await webClient.GetStringAsync(new Uri(RemotePath.Replace(".js", ".min.js")));
                                File.WriteAllText(localPath, file);
                            }

                        }
                    }
                }
                catch (HttpRequestException ex)
                {
                    throw new FileNotFoundException($"The remote file:{RemotePath} cannot be found.", ex);
                }
            }
            if (context.AllAttributes.ContainsName("src"))
            {
                output.CopyHtmlAttribute("src", context);
                if (UseLocal?.Contains(HostingEnvironment.EnvironmentName, StringComparison.OrdinalIgnoreCase) == true)
                    output.Attributes["src"].Value = LocalRelPath;
                string href = output.Attributes["src"].Value.ToString();
                if (UseMinified?.Contains(HostingEnvironment.EnvironmentName, StringComparison.OrdinalIgnoreCase) == true && !href.Contains(".min."))
                    output.Attributes["src"] = href.Replace(".js", ".min.js");
                if (useSiteMinJs)
                    output.Attributes["src"].Value = "";
            }
        }

    }
}