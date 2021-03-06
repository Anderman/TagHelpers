﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using static System.IO.Path;

namespace Anderman.TagHelpers
{
    [HtmlTargetElement("link", Attributes = "asp-fallback-href")]
    [HtmlTargetElement("link", Attributes = "asp-copy-src-to-fallback")]
    [HtmlTargetElement("link", Attributes = "asp-warn-if-test-is-invalid")]
    [HtmlTargetElement("link", Attributes = "asp-use-minified")]

    public class LinkTagHelper : TagHelper
    {
        public static readonly Regex CommentRemove = new Regex(@"(/\*([^*]|[\r\n]|(\*+([^*/]|[\r\n])))*\*+/)",
            RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public static readonly Regex Urls = new Regex(@"url\s*\(\s*['""]?(?<url>[^""')]+)[""']?\s*\)");
        private readonly string _preTest = "\n" + @"
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
    </SCRIPT>".Replace("\n", "").Replace("\r", "") + "\n";

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

        [HtmlAttributeName("asp-use-minified")]
        public string UseMinified { get; set; }

        [HtmlAttributeName("asp-use-site-min-css")]
        public string UseSiteMinCss { get; set; }

        [HtmlAttributeName("asp-use-local")]
        public string UseLocal { get; set; }

        [HtmlAttributeName("asp-warn-if-test-is-invalid")]
        public string WarnIfTestIsInvalid { get; set; }

        protected internal IHostingEnvironment HostingEnvironment { get; set; }
        protected internal IHttpContextAccessor Context { get; set; }
        public LinkTagHelper(IHostingEnvironment hostingEnvironment, IHttpContextAccessor context)
        {
            HostingEnvironment = hostingEnvironment;
            Context = context;
        }
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            bool useSiteMinCss = UseSiteMinCss?.Contains(HostingEnvironment.EnvironmentName, StringComparison.OrdinalIgnoreCase) == true;
            if (WarnIfTestIsInvalid?.Contains(HostingEnvironment.EnvironmentName, StringComparison.OrdinalIgnoreCase) == true)
            {
                await output.GetChildContentAsync();
                //Anly check if not using a minified version 
                if (!useSiteMinCss)
                    output.PreElement.AppendHtml(string.Format(_preTest, FallbackTestClass, FallbackTestProperty, "===", FallbackTestValue, $"Style `{RemotePath}` already loaded. Did you create the correct test"));
                output.PostElement.AppendHtml(string.Format(_preTest, FallbackTestClass, FallbackTestProperty, "!==", FallbackTestValue, $"Style `{RemotePath}` still not loaded. Did you create the correct test"));
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

                    LocalRelPath = (useSiteMinCss == false) ?
                        "/css/css/" + new Uri(RemotePath).Segments.Last()
                        : FallbackSrc ?? "/fallback/css/css/" + new Uri(RemotePath).Segments.Last();
                    var localPath = HostingEnvironment.WebRootPath + LocalRelPath;
                    var pathHelper = new PathHelper(RemotePath, localPath);
                    if (!File.Exists(localPath))
                    {
                        using (var webClient = new HttpClient())
                        {
                            var file = await webClient.GetStringAsync(new Uri(RemotePath));
                            await DownloadDependedCssFiles(file, pathHelper);
                            Directory.CreateDirectory(pathHelper.LocalDirectory);
                            File.WriteAllText(localPath, file);
                            if (RemotePath.Contains(".min.") && UseSiteMinCss != null)//copy full version to let gulp minify this verion to site.css
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
            if (context.AllAttributes.ContainsName("href"))
            {
                output.CopyHtmlAttribute("href", context);
                if (UseLocal?.Contains(HostingEnvironment.EnvironmentName, StringComparison.OrdinalIgnoreCase) == true)
                    output.Attributes.SetAttribute("href",LocalRelPath);
                string href = output.Attributes["href"].Value.ToString();
                if (UseMinified?.Contains(HostingEnvironment.EnvironmentName, StringComparison.OrdinalIgnoreCase) == true && !href.Contains(".min."))
                    output.Attributes.SetAttribute("href", href.Replace(".css", ".min.css"));
                if (useSiteMinCss)
                    output.Attributes.SetAttribute("href", "");
            }
        }

        public Task DownloadDependedCssFiles(string cssContent, PathHelper uriConv)
        {
            var files = GetUniqueFiles(cssContent, uriConv);
            Parallel.ForEach(files, async fileInfo =>
            {
                using (var webClient = new HttpClient())
                {
                    try
                    {
                        var file = await webClient.GetByteArrayAsync(new Uri(fileInfo.RemotePath));
                        Directory.CreateDirectory(GetDirectoryName(fileInfo.LocalPath));
                        File.WriteAllBytes(fileInfo.LocalPath, file);
                    }
                    catch (HttpRequestException ex)
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
                where !url.Groups["url"].Value.Trim(' ').StartsWith("http://", StringComparison.OrdinalIgnoreCase)
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
            LocalDirectory = GetDirectoryName(localPath);
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
            return GetFullPath(Combine(LocalDirectory, relPath));
        }
    }
}