# TagHelpers
ASP.NET MVC 6 Taghelpers

This repo contains TagHelper examples.
  1. PM> Install-Package TagHelpers
  2. Not all fields of the display attribute are supported by mvc 6  so add this option to your startup class
```
    services.AddMvc().Configure<MvcOptions>(m =>
            {
                m.ModelMetadataDetailsProviders.Add(new AdditionalValuesMetadataProvider());
            });
            ```
    This will make `Prompt`, `Description` and `ShortDescription` available to the taghelpers
```
  3. add the taghelper reference to your _viewstart.chtml
  ```
    @addTagHelper "*, TagHelpers"
  ```
  
#Avaiable taghelpers
## inputTagHelper for watermark
###Example
```
  [Display(Name ="Email",Prompt ="Enter email address")]
  public string Email { get; set; }
```
  `<input asp-for="Email"/>`
 
## linkTagHelper and srcTagHelper for CDN with local fallback
If you use CDN for resources files the default taghelpers won't warn you if the configuration is wrong. For instance:
   1. The local version is different from the CDN version
   2. The Check if the CDN is loaded is wrong. It always evalute to true or false
This taghelper will save all files to local and can check if the test attribute is valid.

###Example
```
        <link rel="stylesheet" 
            href="https://ajax.aspnetcdn.com/ajax/bootstrap/3.3.2/css/bootstrap.min.css" 
            asp-fallback-test-class="hidden" 
            asp-fallback-test-property="visibility" 
            asp-fallback-test-value="hidden" 
            asp-copy-src-to-fallback="development" 
            asp-warn-if-test-is-invalid="development" />
```
* `asp-copy-src-to-fallback` will copy the cdn file(s) (inclusive fonts and images) to the wwwroot/fallback directory
* `asp-warn-if-test-is-invalid` will warn you if you used a wrong testvalue to check if CDN file is loaded

## MaterialDesignInputTagHelper
An input taghelper For [boottrap material-design](http://fezvrasta.github.io/bootstrap-material-design)
This taghelper create valid html for material-design. Only the input tag is needed
```
                <input class="form-control" asp-for="Email" asp-material-design="true" />
                <input class="form-control togglebutton" asp-for="EmailConfirmed" asp-material-design="true" />
```

To create form like this ([See datatables sample site](https://github.com/Anderman/Mvc.JQuery.Datatables))

![](http://snag.gy/6IhUP.jpg)

