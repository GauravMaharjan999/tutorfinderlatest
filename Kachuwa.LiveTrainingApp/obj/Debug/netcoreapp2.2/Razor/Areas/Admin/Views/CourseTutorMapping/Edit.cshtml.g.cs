#pragma checksum "F:\Tutor Finder\TutorFinder(BackUp)\Kachuwa.LiveTrainingApp\Areas\Admin\Views\CourseTutorMapping\Edit.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "9b1bb18c6d8ae2746ee922bb32cc8b9b2ccb955e"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.Areas_Admin_Views_CourseTutorMapping_Edit), @"mvc.1.0.view", @"/Areas/Admin/Views/CourseTutorMapping/Edit.cshtml")]
[assembly:global::Microsoft.AspNetCore.Mvc.Razor.Compilation.RazorViewAttribute(@"/Areas/Admin/Views/CourseTutorMapping/Edit.cshtml", typeof(AspNetCore.Areas_Admin_Views_CourseTutorMapping_Edit))]
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
#line 1 "F:\Tutor Finder\TutorFinder(BackUp)\Kachuwa.LiveTrainingApp\Areas\Admin\Views\CourseTutorMapping\Edit.cshtml"
using Kachuwa.Web.Form;

#line default
#line hidden
#line 2 "F:\Tutor Finder\TutorFinder(BackUp)\Kachuwa.LiveTrainingApp\Areas\Admin\Views\CourseTutorMapping\Edit.cshtml"
using Kachuwa.Web.Grid;

#line default
#line hidden
#line 3 "F:\Tutor Finder\TutorFinder(BackUp)\Kachuwa.LiveTrainingApp\Areas\Admin\Views\CourseTutorMapping\Edit.cshtml"
using Kachuwa.Training.Model;

#line default
#line hidden
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"9b1bb18c6d8ae2746ee922bb32cc8b9b2ccb955e", @"/Areas/Admin/Views/CourseTutorMapping/Edit.cshtml")]
    public class Areas_Admin_Views_CourseTutorMapping_Edit : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<CourseTutorMapping>
    {
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
#line 5 "F:\Tutor Finder\TutorFinder(BackUp)\Kachuwa.LiveTrainingApp\Areas\Admin\Views\CourseTutorMapping\Edit.cshtml"
  
    var formDataSource = (FormDatasource)ViewData["FormDataSources"];

#line default
#line hidden
            BeginContext(186, 197, true);
            WriteLiteral("<section class=\"container-fluid\">\r\n    <div class=\"row\">\r\n        <div class=\"col-wrapper col-lg-12 col-md-12 col-sm-12 col-xs-12\">\r\n\r\n            <div class=\"cardform-wrapper\">\r\n\r\n                ");
            EndContext();
            BeginContext(385, 4777, false);
#line 14 "F:\Tutor Finder\TutorFinder(BackUp)\Kachuwa.LiveTrainingApp\Areas\Admin\Views\CourseTutorMapping\Edit.cshtml"
            Write(
                                                                                                            Html.CreateKachuwaForm<CourseTutorMapping>("CourseTutorMappingForm", Model)
                                                                                                            .SetHeading("")
                                                                                                                .ActionUrl("/admin/coursetutormapping/edit")
                                                                                                                .CancelUrl("/admin/coursetutormapping")
                                                                                                                .EncType("multipart/form-data")
                                                                                                            .CreateSection(section =>
                                                                                                            {
                                                                                                                section.Add("section1", "cardform-section", rows =>
                                                                                                                {
                                                                                                                    rows.Add("1strow", "row", columns =>
                                                                                                                {
                                                                                                                    columns.Add("left", "col-md-4", item => new global::Microsoft.AspNetCore.Mvc.Razor.HelperResult(async(__razor_template_writer) => {
    PushWriter(__razor_template_writer);
    BeginContext(2097, 459, true);
    WriteLiteral(@"<div class=""form-description"">
                                                                                                                        <h4>Course Tutor Mapping</h4>
                                                                                                                        <p>Edit Course Tutor Mapping</p>
                                                                                                                    </div>");
    EndContext();
    PopWriter();
}
));




                                                                                                                    columns.Add("right", "col-md-8", control =>
                                                                                                                    {
                                                                                                                        control.Add("form-control", model => model.Id, FormInputControl.Hidden);

                                                                                                                        control.Add("form-control  ", model => model.CourseId, FormInputControl.Select, formDataSource.GetSource("CourseListSource"))
                                                                                                                        .SetFirstParentClass("form-group")
                                                                                                                        .SetSecondParentClass("col-md-12")
                                                                                                                        .SetDisplayName("Choose Course");

                                                                                                                        control.Add("form-control  ", model => model.TutorId, FormInputControl.Select, formDataSource.GetSource("TutorListSource"))
                                                                                                                      .SetFirstParentClass("form-group")
                                                                                                                      .SetSecondParentClass("col-md-12")
                                                                                                                      .SetDisplayName("Choose Tutor");



                                                                                                                    }).SetFirstChildClass("forminput-section").SetSecondChildClass("row");
                                                                                                                });
                                                                                                                })
                                                                                                                     .SetHeading("Edit Course Tutor Mapping ");
                                                                                                            }));

#line default
#line hidden
            EndContext();
            BeginContext(5164, 64, true);
            WriteLiteral("\r\n            </div>\r\n        </div>\r\n    </div>\r\n</section>\r\n\r\n");
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
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<CourseTutorMapping> Html { get; private set; }
    }
}
#pragma warning restore 1591
