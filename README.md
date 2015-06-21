# TagHelpers
ASP.NET MVC 6 Taghelpers

This repo contains TagHelper examples.
Current TagHelpers
## inputTagHelper  watermark added
###Example
  &lt;input asp-for="Email"  />
 
## linkTagHelper Autosave fallback url
This should maintainance of both urls easier. 
###Example
    <script src="http://ajax.aspnetcdn.com/ajax/jquery/jquery-1.11.2.min.js"
            asp-fallback-src="~/fallback/js/jquery.js"
            asp-fallback-test="window.jQuery">
    </script>  

