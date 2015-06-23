using System.Text.RegularExpressions;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Loader.IIS;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;
using System.Linq;

namespace DBC.Helpers
{
    [TargetElement("link", Attributes = "asp-fallback-href")]
    public class LinkTagHelper : TagHelper
    {
        [HtmlAttributeName("asp-fallback-href")]
        public string FallbackSrc { get; set; }

        [HtmlAttributeName("href")]
        public string remotePath { get; set; }

        [Activate]
        protected internal IHostingEnvironment HostingEnvironment { get; set; }
        [Activate]
        protected internal IHttpContextAccessor Context { get; set; }


        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            try
            {
                if (HostingEnvironment.EnvironmentName == "Development")
                {
                    remotePath = remotePath.StartsWith("/") ? Context.HttpContext.Request.Scheme + "://" + Context.HttpContext.Request.Host + remotePath : remotePath;
                    var localPath = HostingEnvironment.MapPath(FallbackSrc.TrimStart('/'));
                    var pathHelper = new PathHelper(remotePath, localPath);
                    if (!File.Exists(localPath))
                    {
                        using (var webClient = new WebClient())
                        {
                            var file = await webClient.DownloadStringTaskAsync(new Uri(remotePath));
                            await DownloadUrls(file, pathHelper);
                            Directory.CreateDirectory(pathHelper.LocalDirectory);
                            File.WriteAllText(localPath, file);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"URI {remotePath}", ex);
            }
        }
        public static readonly Regex CommentRemove = new Regex(@"(/\*([^*]|[\r\n]|(\*+([^*/]|[\r\n])))*\*+/)", RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.CultureInvariant);
        public static readonly Regex Urls = new Regex(@"url\s*\(\s*['""]?(?<url>[^""')]+)[""']?\s*\)");
        public async Task DownloadUrls(string cssContent, PathHelper uriConv)
        {
            cssContent = CommentRemove.Replace(cssContent, "");
            var urls =
                Regex.Matches(cssContent, @"url\s*\(\s*['""]?(?<url>[^""')]+)[""']?\s*\)")
                    .Cast<Match>()
                    .Select(m => m.Groups["url"].Value).Distinct();
            Parallel.ForEach(urls, url =>
            {
                using (var webClient = new WebClient())
                {
                    var relRemotePath = url;
                    var remotePath = uriConv.GetRemotePath(relRemotePath);
                    var localPath = uriConv.GetLocalPath(relRemotePath);
                    try
                    {
                        var file = webClient.DownloadData(new Uri(remotePath));
                        Directory.CreateDirectory(Path.GetDirectoryName(localPath));
                        File.WriteAllBytes(localPath, file);
                    }
                    catch (IOException ex)
                    {
                    }
                    catch (WebException ex)
                    {
                        throw new Exception($"remotepath:{remotePath} cannot be found.", ex);
                    }
                }
            });
        }
    }
    public class PathHelper
    {
        private readonly string _remoteDirectory;
        public readonly string LocalDirectory;


        public PathHelper(string remotePath, string localPath)
        {
            _remoteDirectory = remotePath.Substring(0, remotePath.LastIndexOf("/") + 1);
            LocalDirectory = Path.GetDirectoryName(localPath);
        }
        public string GetRemotePath(string relativeRemotePath)
        {
            return _remoteDirectory + relativeRemotePath;
        }
        public string GetLocalPath(string relativeRemotePath)
        {
            var relPath = relativeRemotePath.Contains("?") ? relativeRemotePath.Substring(0, relativeRemotePath.LastIndexOf("?")) : relativeRemotePath;
            relPath = relPath.Contains("#") ? relPath.Substring(0, relPath.LastIndexOf("#")) : relPath;
            return Path.GetFullPath(Path.Combine(LocalDirectory, relPath));
        }
    }

}
