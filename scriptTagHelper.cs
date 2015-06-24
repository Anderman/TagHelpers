using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;

namespace DBC.Helpers
{
    [TargetElement("link", Attributes = "asp-fallback-href")]
    public class LinkTagHelper : TagHelper
    {
        private readonly string preTest = @"
    <meta name='x-stylesheet-fallback-test' class='{0}'>
    <SCRIPT>
        !function() {{ 
            var d, e = document, 
            f = e.getElementsByTagName('SCRIPT'), 
            g = f[f.length - 1].previousElementSibling, 
            h = e.defaultView && e.defaultView.getComputedStyle 
                ? e.defaultView.getComputedStyle(g) 
                : g.currentStyle; 
            if (h && h['{1}'] {2} '{3}') 
                alert('{4}'); 
        }}();
    </SCRIPT>";
        public static readonly Regex CommentRemove = new Regex(@"(/\*([^*]|[\r\n]|(\*+([^*/]|[\r\n])))*\*+/)",
            RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public static readonly Regex Urls = new Regex(@"url\s*\(\s*['""]?(?<url>[^""')]+)[""']?\s*\)");
        [HtmlAttributeName("asp-fallback-test-class")]
        public string FallbackTestClass { get; set; }

        [HtmlAttributeName("asp-fallback-test-property")]
        public string FallbackTestProperty { get; set; }

        [HtmlAttributeName("asp-fallback-test-value")]
        public string FallbackTestValue { get; set; }

        [HtmlAttributeName("asp-fallback-href")]
        public string FallbackSrc { get; set; }

        [HtmlAttributeName("href")]
        public string RemotePath { get; set; }

        [HtmlAttributeName("asp-copy-src-to-fallback")]
        public string CopySrcToFallback { get; set; }

        [HtmlAttributeName("asp-warn-if-test-is-invalid")]
        public string WarnIfTestIsInvalid { get; set; }

        [Activate]
        protected internal IHostingEnvironment HostingEnvironment { get; set; }

        [Activate]
        protected internal IHttpContextAccessor Context { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (WarnIfTestIsInvalid?.Contains(HostingEnvironment.EnvironmentName, StringComparison.OrdinalIgnoreCase) == true)
            {
                output.PreContent.Append(string.Format(preTest, FallbackTestClass, FallbackTestProperty, "===", FallbackTestValue, $"Style `{RemotePath}` already loaded. Did you create the correct test"));
                output.PostContent.Append(string.Format(preTest, FallbackTestClass, FallbackTestProperty, "!==", FallbackTestValue, $"Style `{RemotePath}` still not loaded. Did you create the correct test"));
            }

            if (CopySrcToFallback?.Contains(HostingEnvironment.EnvironmentName, StringComparison.OrdinalIgnoreCase)==true)
            {
                try
                {
                    RemotePath = RemotePath.StartsWith("/")
                        ? Context.HttpContext.Request.Scheme + "://" + Context.HttpContext.Request.Host + RemotePath
                        : RemotePath;
                    var localPath = HostingEnvironment.MapPath(FallbackSrc.TrimStart('/'));
                    var pathHelper = new PathHelper(RemotePath, localPath);
                    if (!File.Exists(localPath))
                    {
                        using (var webClient = new WebClient())
                        {
                            var file = await webClient.DownloadStringTaskAsync(new Uri(RemotePath));
                            await DownloadDependedCssFiles(file, pathHelper);
                            Directory.CreateDirectory(pathHelper.LocalDirectory);
                            File.WriteAllText(localPath, file);
                        }
                    }
                }
                catch (WebException ex)
                {
                    throw new FileNotFoundException($"The remote file:{RemotePath} cannot be found.", ex);
                }
            }
        }

        public Task DownloadDependedCssFiles(string cssContent, PathHelper uriConv)
        {
            var files = GetUniqueFiles(cssContent, uriConv);
            Parallel.ForEach(files, fileInfo =>
            {
                using (var webClient = new WebClient())
                {
                    try
                    {
                        var file = webClient.DownloadData(new Uri(fileInfo.RemotePath));
                        Directory.CreateDirectory(Path.GetDirectoryName(fileInfo.LocalPath));
                        File.WriteAllBytes(fileInfo.LocalPath, file);
                    }
                    catch (WebException ex)
                    {
                        throw new FileNotFoundException($"The remote file:{fileInfo.RemotePath} cannot be found.", ex);
                    }
                }
            });
            return Task.FromResult(0);
        }

        //Ignore duplicate urls and fontface tricks like '../fonts/fontawesome-webfont.eot?#iefix&v=4.3.0'
        //But still downloading the correct version
        public static IEnumerable<FileInfo> GetUniqueFiles(string cssContent, PathHelper uriConv)
        {
            cssContent = CommentRemove.Replace(cssContent, "");
            var urls =
                from Match url in Urls.Matches(cssContent)
                group url by uriConv.GetLocalPath(url.Groups["url"].Value)
                into g
                select
                    new FileInfo { RemotePath = uriConv.GetRemotePath(g.First().Groups["url"].Value), LocalPath = g.Key };
            return urls;
        }

        public class FileInfo
        {
            public string RemotePath { get; set; }
            public string LocalPath { get; set; }
        }
    }
    public static class StringExtensions
    {
        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source.IndexOf(toCheck, comp) >= 0;
        }
    }
    public class PathHelper
    {
        private readonly string _remoteDirectory;
        public readonly string LocalDirectory;

        public PathHelper(string remotePath, string localPath)
        {
            _remoteDirectory = remotePath.Substring(0, remotePath.LastIndexOf("/", StringComparison.Ordinal) + 1);
            LocalDirectory = Path.GetDirectoryName(localPath);
        }

        public string GetRemotePath(string relativeRemotePath)
        {
            return _remoteDirectory + relativeRemotePath;
        }

        public string GetLocalPath(string relativeRemotePath)
        {
            var relPath = relativeRemotePath.Contains("?")
                ? relativeRemotePath.Substring(0, relativeRemotePath.LastIndexOf("?", StringComparison.Ordinal))
                : relativeRemotePath;
            relPath = relPath.Contains("#")
                ? relPath.Substring(0, relPath.LastIndexOf("#", StringComparison.Ordinal))
                : relPath;
            return Path.GetFullPath(Path.Combine(LocalDirectory, relPath));
        }
    }
}