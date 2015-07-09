using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Anderman.TagHelpers;
namespace TagHelpers.test
{
    public class DownloadUrl
    {
        [Fact]
        public void DownloadUrlTest()
        {
            var pathHelper = new PathHelper("http://maxcdn.bootstrapcdn.com/font-awesome/4.3.0/css/font-awesome.min.css", @"C:\git\t2\DBC\src\DBC\wwwroot\fallback\css\bootstrap.min.css");
            pathHelper.GetRemotePath("../fonts/fontawesome-webfont.eot?v=4.3.0");
            Assert.Equal(pathHelper.GetRemotePath("../fonts/fontawesome-webfont.eot?v=4.3.0"), "http://maxcdn.bootstrapcdn.com/font-awesome/4.3.0/css/../fonts/fontawesome-webfont.eot?v=4.3.0");
            Assert.Equal(pathHelper.GetLocalPath("../fonts/fontawesome-webfont.eot?v=4.3.0"), @"C:\git\t2\DBC\src\DBC\wwwroot\fallback\fonts\fontawesome-webfont.eot");


             string cssContent = @"
                @font - face {
                    font - family: 'FontAwesome';
                    src: url('../fonts/fontawesome-webfont.eot?v=4.3.0');
                    src: url( '../fonts/fontawesome-webfont.eot?#iefix&v=4.3.0') format('embedded-opentype'), 
                        url(""../fonts/fontawesome-webfont.woff2?v=4.3.0') format('woff2""), 
                        url ( '../fonts/fontawesome-webfont.woff?v=4.3.0') format('woff'), 
                        url('../fonts/fontawesome-webfont.ttf?v=4.3.0' ) format('truetype'), 
                        url('../fonts/fontawesome-webfont.svg?v=4.3.0#fontawesomeregular') format('svg');
                    font - weight: normal;
                    font - style: normal;
                }";
            var urls = LinkTagHelper.Urls.Matches(cssContent);
            Assert.Equal(urls.Count, 6);

            cssContent = @"this is some stuff right here
                /* blah blah blah 
                   blah blah blah 
                   blah blah blah */ and this is more stuff /* blah */
                   right here stuff.";
            var result = LinkTagHelper.CommentRemove.Replace(cssContent,"");
            Assert.DoesNotContain(result, "bla");
            Assert.Contains(result, "stuff");
        }

    }
}
