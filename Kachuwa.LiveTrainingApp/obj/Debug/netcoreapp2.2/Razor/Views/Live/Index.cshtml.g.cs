#pragma checksum "F:\Tutor Finder\TutorFinder(BackUp)\Kachuwa.LiveTrainingApp\Views\Live\Index.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "9b760cf42beea2bd44fad9689437a390e89b3076"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.Views_Live_Index), @"mvc.1.0.view", @"/Views/Live/Index.cshtml")]
[assembly:global::Microsoft.AspNetCore.Mvc.Razor.Compilation.RazorViewAttribute(@"/Views/Live/Index.cshtml", typeof(AspNetCore.Views_Live_Index))]
namespace AspNetCore
{
    #line hidden
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
#line 1 "F:\Tutor Finder\TutorFinder(BackUp)\Kachuwa.LiveTrainingApp\Views\_ViewImports.cshtml"
using Kachuwa.KLiveApp;

#line default
#line hidden
#line 2 "F:\Tutor Finder\TutorFinder(BackUp)\Kachuwa.LiveTrainingApp\Views\_ViewImports.cshtml"
using Kachuwa.KLiveApp.Models;

#line default
#line hidden
#line 1 "F:\Tutor Finder\TutorFinder(BackUp)\Kachuwa.LiveTrainingApp\Views\Live\Index.cshtml"
using Kachuwa.Dash.Live;

#line default
#line hidden
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"9b760cf42beea2bd44fad9689437a390e89b3076", @"/Views/Live/Index.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"64dbc793665767572784b5fa67e39f634b920016", @"/Views/_ViewImports.cshtml")]
    public class Views_Live_Index : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<IEnumerable<Kachuwa.Dash.Live.LiveStreamDetailViewModel>>
    {
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
#line 3 "F:\Tutor Finder\TutorFinder(BackUp)\Kachuwa.LiveTrainingApp\Views\Live\Index.cshtml"
  
    ViewData["Title"] = "Lives";

#line default
#line hidden
            BeginContext(133, 979, true);
            WriteLiteral(@"
<!-- Inner Page Breadcrumb -->
<section class=""inner_page_breadcrumb"">
    <div class=""container"">
        <div class=""row"">
            <div class=""col-xl-6 offset-xl-3 text-center"">
                <div class=""breadcrumb_content"">
                    <h4 class=""breadcrumb_title"">Courses</h4>
                    <ol class=""breadcrumb"">
                        <li class=""breadcrumb-item""><a href=""#"">Home</a></li>
                        <li class=""breadcrumb-item active"" aria-current=""page"">Live Classes</li>
                    </ol>
                </div>
            </div>
        </div>
    </div>
</section>


<div class=""row p-4"">
    <button><a href=""www.facebook.com""> Join Zoom Class</a></button>
</div>
<!-- Courses List 2 -->
<section class=""courses-list2 pb40"">
    <div class=""container"">
        <div class=""row"">
            <div class=""col-md-12 col-lg-8 col-xl-9"">
                <div class=""row courses_list_heading style2"">
");
            EndContext();
            BeginContext(2640, 90, true);
            WriteLiteral("                </div>\r\n\r\n\r\n                <div class=\"row courses_container style2\">\r\n\r\n");
            EndContext();
#line 65 "F:\Tutor Finder\TutorFinder(BackUp)\Kachuwa.LiveTrainingApp\Views\Live\Index.cshtml"
                     foreach (var item in Model)
                    {

#line default
#line hidden
            BeginContext(2803, 297, true);
            WriteLiteral(@"                        <div class=""col-lg-12 p0"">
                            <div class=""courses_list_content"">
                                <div class=""top_courses list"">
                                    <div class=""thumb"">
                                        <img class=""img-whp""");
            EndContext();
            BeginWriteAttribute("src", " src=\"", 3100, "\"", 3128, 1);
#line 71 "F:\Tutor Finder\TutorFinder(BackUp)\Kachuwa.LiveTrainingApp\Views\Live\Index.cshtml"
WriteAttributeValue("", 3106, item.ProfileImagePath, 3106, 22, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginContext(3129, 256, true);
            WriteLiteral(@" alt=""t1.jpg"">
                                        <div class=""overlay"">
                                            <div class=""icon""><span class=""flaticon-like""></span></div>
                                            <a class=""tc_preview_course""");
            EndContext();
            BeginWriteAttribute("href", " href=\"", 3385, "\"", 3414, 2);
            WriteAttributeValue("", 3392, "/course/", 3392, 8, true);
#line 74 "F:\Tutor Finder\TutorFinder(BackUp)\Kachuwa.LiveTrainingApp\Views\Live\Index.cshtml"
WriteAttributeValue("", 3400, item.CourseId, 3400, 14, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginContext(3415, 285, true);
            WriteLiteral(@">Preview Course</a>
                                        </div>
                                    </div>
                                    <div class=""details"">
                                        <div class=""tc_content"">
                                            <p>");
            EndContext();
            BeginContext(3701, 14, false);
#line 79 "F:\Tutor Finder\TutorFinder(BackUp)\Kachuwa.LiveTrainingApp\Views\Live\Index.cshtml"
                                          Write(item.TutorName);

#line default
#line hidden
            EndContext();
            BeginContext(3715, 54, true);
            WriteLiteral("</p>\r\n                                            <h5>");
            EndContext();
            BeginContext(3770, 15, false);
#line 80 "F:\Tutor Finder\TutorFinder(BackUp)\Kachuwa.LiveTrainingApp\Views\Live\Index.cshtml"
                                           Write(item.CourseName);

#line default
#line hidden
            EndContext();
            BeginContext(3785, 1, true);
            WriteLiteral(" ");
            EndContext();
            BeginContext(3826, 54, true);
            WriteLiteral("</h5>\r\n                                            <p>");
            EndContext();
            BeginContext(3881, 27, false);
#line 81 "F:\Tutor Finder\TutorFinder(BackUp)\Kachuwa.LiveTrainingApp\Views\Live\Index.cshtml"
                                          Write(item.CourseShortDescription);

#line default
#line hidden
            EndContext();
            BeginContext(3908, 424, true);
            WriteLiteral(@"</p>
                                        </div>
                                        <div class=""tc_footer"">
                                            <ul class=""tc_meta float-left fn-414"">
                                                <li class=""list-inline-item""><a href=""#""><i class=""flaticon-profile""></i></a></li>
                                                <li class=""list-inline-item""><a href=""#"">");
            EndContext();
            BeginContext(4333, 20, false);
#line 86 "F:\Tutor Finder\TutorFinder(BackUp)\Kachuwa.LiveTrainingApp\Views\Live\Index.cshtml"
                                                                                    Write(item.LiveViewerCount);

#line default
#line hidden
            EndContext();
            BeginContext(4353, 238, true);
            WriteLiteral(" watching</a></li>\r\n                                                <li class=\"list-inline-item\"><a href=\"#\"><i class=\"flaticon-like\"></i></a></li>\r\n                                                <li class=\"list-inline-item\"><a href=\"#\">");
            EndContext();
            BeginContext(4592, 10, false);
#line 88 "F:\Tutor Finder\TutorFinder(BackUp)\Kachuwa.LiveTrainingApp\Views\Live\Index.cshtml"
                                                                                    Write(item.Likes);

#line default
#line hidden
            EndContext();
            BeginContext(4602, 62, true);
            WriteLiteral("</a></li>\r\n                                            </ul>\r\n");
            EndContext();
            BeginContext(5586, 225, true);
            WriteLiteral("                                        </div>\r\n                                        <div>\r\n                                            <a type=\"button\" class=\"btn btn-success button play flaticon-play-button-1 pull-right\"");
            EndContext();
            BeginWriteAttribute("href", " href=\"", 5811, "\"", 5840, 2);
            WriteAttributeValue("", 5818, "/watch?v=", 5818, 9, true);
#line 101 "F:\Tutor Finder\TutorFinder(BackUp)\Kachuwa.LiveTrainingApp\Views\Live\Index.cshtml"
WriteAttributeValue("", 5827, item.VideoId, 5827, 13, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginContext(5841, 231, true);
            WriteLiteral("> <span>Watch Now</span> </a>\r\n                                        </div>\r\n                                    </div>\r\n                                </div>\r\n                            </div>\r\n                        </div>\r\n");
            EndContext();
#line 107 "F:\Tutor Finder\TutorFinder(BackUp)\Kachuwa.LiveTrainingApp\Views\Live\Index.cshtml"
                    }

#line default
#line hidden
            BeginContext(6095, 90, true);
            WriteLiteral("\r\n                </div>\r\n\r\n                <div class=\"row courses_container style2\">\r\n\r\n");
            EndContext();
#line 113 "F:\Tutor Finder\TutorFinder(BackUp)\Kachuwa.LiveTrainingApp\Views\Live\Index.cshtml"
                     foreach (var item in ViewBag.EventData)
                    {

#line default
#line hidden
            BeginContext(6270, 297, true);
            WriteLiteral(@"                        <div class=""col-lg-12 p0"">
                            <div class=""courses_list_content"">
                                <div class=""top_courses list"">
                                    <div class=""thumb"">
                                        <img class=""img-whp""");
            EndContext();
            BeginWriteAttribute("src", " src=\"", 6567, "\"", 6595, 1);
#line 119 "F:\Tutor Finder\TutorFinder(BackUp)\Kachuwa.LiveTrainingApp\Views\Live\Index.cshtml"
WriteAttributeValue("", 6573, item.ProfileImagePath, 6573, 22, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginContext(6596, 259, true);
            WriteLiteral(@" alt=""event.jpg"">
                                        <div class=""overlay"">
                                            <div class=""icon""><span class=""flaticon-like""></span></div>
                                            <a class=""tc_preview_course""");
            EndContext();
            BeginWriteAttribute("href", " href=\"", 6855, "\"", 6882, 2);
            WriteAttributeValue("", 6862, "/event/", 6862, 7, true);
#line 122 "F:\Tutor Finder\TutorFinder(BackUp)\Kachuwa.LiveTrainingApp\Views\Live\Index.cshtml"
WriteAttributeValue("", 6869, item.EventId, 6869, 13, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginContext(6883, 237, true);
            WriteLiteral(">Preview Event</a>\r\n                                        </div>\r\n                                    </div>\r\n                                    <div class=\"details\">\r\n                                        <div class=\"tc_content\">\r\n");
            EndContext();
            BeginContext(7192, 48, true);
            WriteLiteral("                                            <h5>");
            EndContext();
            BeginContext(7241, 15, false);
#line 128 "F:\Tutor Finder\TutorFinder(BackUp)\Kachuwa.LiveTrainingApp\Views\Live\Index.cshtml"
                                           Write(item.EventTitle);

#line default
#line hidden
            EndContext();
            BeginContext(7256, 55, true);
            WriteLiteral(" </h5>\r\n                                            <p>");
            EndContext();
            BeginContext(7312, 26, false);
#line 129 "F:\Tutor Finder\TutorFinder(BackUp)\Kachuwa.LiveTrainingApp\Views\Live\Index.cshtml"
                                          Write(item.EventShortDescription);

#line default
#line hidden
            EndContext();
            BeginContext(7338, 424, true);
            WriteLiteral(@"</p>
                                        </div>
                                        <div class=""tc_footer"">
                                            <ul class=""tc_meta float-left fn-414"">
                                                <li class=""list-inline-item""><a href=""#""><i class=""flaticon-profile""></i></a></li>
                                                <li class=""list-inline-item""><a href=""#"">");
            EndContext();
            BeginContext(7763, 20, false);
#line 134 "F:\Tutor Finder\TutorFinder(BackUp)\Kachuwa.LiveTrainingApp\Views\Live\Index.cshtml"
                                                                                    Write(item.LiveViewerCount);

#line default
#line hidden
            EndContext();
            BeginContext(7783, 238, true);
            WriteLiteral(" watching</a></li>\r\n                                                <li class=\"list-inline-item\"><a href=\"#\"><i class=\"flaticon-like\"></i></a></li>\r\n                                                <li class=\"list-inline-item\"><a href=\"#\">");
            EndContext();
            BeginContext(8022, 10, false);
#line 136 "F:\Tutor Finder\TutorFinder(BackUp)\Kachuwa.LiveTrainingApp\Views\Live\Index.cshtml"
                                                                                    Write(item.Likes);

#line default
#line hidden
            EndContext();
            BeginContext(8032, 287, true);
            WriteLiteral(@"</a></li>
                                            </ul>
                                        </div>
                                        <div>
                                            <a type=""button"" class=""btn btn-success button play flaticon-play-button-1 pull-right""");
            EndContext();
            BeginWriteAttribute("href", " href=\"", 8319, "\"", 8348, 2);
            WriteAttributeValue("", 8326, "/watch?v=", 8326, 9, true);
#line 140 "F:\Tutor Finder\TutorFinder(BackUp)\Kachuwa.LiveTrainingApp\Views\Live\Index.cshtml"
WriteAttributeValue("", 8335, item.VideoId, 8335, 13, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginContext(8349, 231, true);
            WriteLiteral("> <span>Watch Now</span> </a>\r\n                                        </div>\r\n                                    </div>\r\n                                </div>\r\n                            </div>\r\n                        </div>\r\n");
            EndContext();
#line 146 "F:\Tutor Finder\TutorFinder(BackUp)\Kachuwa.LiveTrainingApp\Views\Live\Index.cshtml"
                    }

#line default
#line hidden
            BeginContext(8603, 34, true);
            WriteLiteral("\r\n\r\n\r\n\r\n\r\n                </div>\r\n");
            EndContext();
            BeginContext(9892, 33, true);
            WriteLiteral("            </div>\r\n\r\n         \r\n");
            EndContext();
            BeginContext(26971, 38, true);
            WriteLiteral("        </div>\r\n    </div>\r\n</section>");
            EndContext();
        }
        #pragma warning restore 1998
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.ViewFeatures.IModelExpressionProvider ModelExpressionProvider { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IUrlHelper Url { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IViewComponentHelper Component { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IJsonHelper Json { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<IEnumerable<Kachuwa.Dash.Live.LiveStreamDetailViewModel>> Html { get; private set; }
    }
}
#pragma warning restore 1591
