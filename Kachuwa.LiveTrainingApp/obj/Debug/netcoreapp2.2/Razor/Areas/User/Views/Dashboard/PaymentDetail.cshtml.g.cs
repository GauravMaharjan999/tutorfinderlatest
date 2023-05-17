#pragma checksum "F:\Tutor Finder\TutorFinder(BackUp)\Kachuwa.LiveTrainingApp\Areas\User\Views\Dashboard\PaymentDetail.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "100bdd141948948c6763f5862aa5ad72a907a56b"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.Areas_User_Views_Dashboard_PaymentDetail), @"mvc.1.0.view", @"/Areas/User/Views/Dashboard/PaymentDetail.cshtml")]
[assembly:global::Microsoft.AspNetCore.Mvc.Razor.Compilation.RazorViewAttribute(@"/Areas/User/Views/Dashboard/PaymentDetail.cshtml", typeof(AspNetCore.Areas_User_Views_Dashboard_PaymentDetail))]
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
#line 1 "F:\Tutor Finder\TutorFinder(BackUp)\Kachuwa.LiveTrainingApp\Areas\User\Views\_ViewImports.cshtml"
using Kachuwa.KLiveApp;

#line default
#line hidden
#line 2 "F:\Tutor Finder\TutorFinder(BackUp)\Kachuwa.LiveTrainingApp\Areas\User\Views\_ViewImports.cshtml"
using Kachuwa.KLiveApp.Models;

#line default
#line hidden
#line 1 "F:\Tutor Finder\TutorFinder(BackUp)\Kachuwa.LiveTrainingApp\Areas\User\Views\Dashboard\PaymentDetail.cshtml"
using Kachuwa.Training.Model;

#line default
#line hidden
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"100bdd141948948c6763f5862aa5ad72a907a56b", @"/Areas/User/Views/Dashboard/PaymentDetail.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"64dbc793665767572784b5fa67e39f634b920016", @"/Areas/User/Views/_ViewImports.cshtml")]
    public class Areas_User_Views_Dashboard_PaymentDetail : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<PaymentLogViewModel>
    {
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
            BeginContext(59, 2, true);
            WriteLiteral("\r\n");
            EndContext();
#line 4 "F:\Tutor Finder\TutorFinder(BackUp)\Kachuwa.LiveTrainingApp\Areas\User\Views\Dashboard\PaymentDetail.cshtml"
  
    ViewData["Title"] = "Payment Detail";

#line default
#line hidden
            BeginContext(111, 271, true);
            WriteLiteral(@"

<div class=""col-lg-12"">
    <div class=""my_course_content_container"">
        <div class=""my_setting_content"">
            <div class=""my_setting_content_header"">
                <div class=""my_sch_title"">
                    <h4 class=""m0"">Payment Detail</h4>
");
            EndContext();
#line 15 "F:\Tutor Finder\TutorFinder(BackUp)\Kachuwa.LiveTrainingApp\Areas\User\Views\Dashboard\PaymentDetail.cshtml"
                     if (Model.IsVerified != true)
                    {

#line default
#line hidden
            BeginContext(457, 108, true);
            WriteLiteral("                        <span style=\" color : red\" class=\"pull-right\"> &#10060;Payment Not Verified</span>\r\n");
            EndContext();
#line 18 "F:\Tutor Finder\TutorFinder(BackUp)\Kachuwa.LiveTrainingApp\Areas\User\Views\Dashboard\PaymentDetail.cshtml"
                    }
                    else
                    {

#line default
#line hidden
            BeginContext(637, 105, true);
            WriteLiteral("                        <span style=\" color : green\" class=\"pull-right\">&#9989; Payment Verified</span>\r\n");
            EndContext();
#line 22 "F:\Tutor Finder\TutorFinder(BackUp)\Kachuwa.LiveTrainingApp\Areas\User\Views\Dashboard\PaymentDetail.cshtml"
                    }

#line default
#line hidden
            BeginContext(765, 854, true);
            WriteLiteral(@"                </div>
            </div>


            <div class=""my_setting_content_details pb0"">

                <div class=""col-md-12"">
                    <div class=""row"">
                        <div class=""col-md-6"">
                            <h4 class=""title""> Voucher Details</h4>
                            <table class=""table table-bordered table-striped"">
                                <thead>
                                    <tr>
                                        <th>Voucher Number</th>
                                        <th>Deposited By</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    <tr>
                                        <td>
                                            ");
            EndContext();
            BeginContext(1620, 19, false);
#line 43 "F:\Tutor Finder\TutorFinder(BackUp)\Kachuwa.LiveTrainingApp\Areas\User\Views\Dashboard\PaymentDetail.cshtml"
                                       Write(Model.VoucherNumber);

#line default
#line hidden
            EndContext();
            BeginContext(1639, 139, true);
            WriteLiteral("\r\n                                        </td>\r\n                                        <td>\r\n                                            ");
            EndContext();
            BeginContext(1779, 17, false);
#line 46 "F:\Tutor Finder\TutorFinder(BackUp)\Kachuwa.LiveTrainingApp\Areas\User\Views\Dashboard\PaymentDetail.cshtml"
                                       Write(Model.DepositedBy);

#line default
#line hidden
            EndContext();
            BeginContext(1796, 810, true);
            WriteLiteral(@"
                                        </td>
                                    </tr>
                                </tbody>

                            </table>

                            <h4 class=""title"">Course Detail</h4>
                            <table class=""table table-bordered table-striped"">
                                <thead>
                                    <tr>
                                        <th width=""70%"">Course Name</th>
                                        <th width=""30%"">Course Fee</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    <tr>
                                        <td>
                                            ");
            EndContext();
            BeginContext(2607, 16, false);
#line 64 "F:\Tutor Finder\TutorFinder(BackUp)\Kachuwa.LiveTrainingApp\Areas\User\Views\Dashboard\PaymentDetail.cshtml"
                                       Write(Model.CourseName);

#line default
#line hidden
            EndContext();
            BeginContext(2623, 139, true);
            WriteLiteral("\r\n                                        </td>\r\n                                        <td>\r\n                                            ");
            EndContext();
            BeginContext(2763, 15, false);
#line 67 "F:\Tutor Finder\TutorFinder(BackUp)\Kachuwa.LiveTrainingApp\Areas\User\Views\Dashboard\PaymentDetail.cshtml"
                                       Write(Model.CourseFee);

#line default
#line hidden
            EndContext();
            BeginContext(2778, 404, true);
            WriteLiteral(@"
                                        </td>
                                    </tr>
                                </tbody>

                            </table>

                        </div>

                        <div class=""col-md-6"">
                            <div class=""order_sidebar_widget mb30"">
                                <h4 class=""title"">Bank Voucher Preview</h4>
");
            EndContext();
#line 79 "F:\Tutor Finder\TutorFinder(BackUp)\Kachuwa.LiveTrainingApp\Areas\User\Views\Dashboard\PaymentDetail.cshtml"
                                 if (Model.VoucherAttachmentPath != "/")
                                {

#line default
#line hidden
            BeginContext(3291, 51, true);
            WriteLiteral("                                    <img id=\"smimg\"");
            EndContext();
            BeginWriteAttribute("src", " src=\"", 3342, "\"", 3376, 1);
#line 81 "F:\Tutor Finder\TutorFinder(BackUp)\Kachuwa.LiveTrainingApp\Areas\User\Views\Dashboard\PaymentDetail.cshtml"
WriteAttributeValue("", 3348, Model.VoucherAttachmentPath, 3348, 28, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginContext(3377, 30, true);
            WriteLiteral(" height=\"400\" width=\"650\" />\r\n");
            EndContext();
#line 82 "F:\Tutor Finder\TutorFinder(BackUp)\Kachuwa.LiveTrainingApp\Areas\User\Views\Dashboard\PaymentDetail.cshtml"
                                }
                                else
                                {

#line default
#line hidden
            BeginContext(3515, 85, true);
            WriteLiteral("                                    <span><br /> No Voucher Attachment found</span>\r\n");
            EndContext();
#line 86 "F:\Tutor Finder\TutorFinder(BackUp)\Kachuwa.LiveTrainingApp\Areas\User\Views\Dashboard\PaymentDetail.cshtml"
                                }

#line default
#line hidden
            BeginContext(3635, 188, true);
            WriteLiteral("\r\n                            </div>\r\n\r\n                        </div>\r\n\r\n                    </div>\r\n                </div>\r\n\r\n            </div>\r\n        </div>\r\n    </div>\r\n</div>\r\n\r\n\r\n");
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
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<PaymentLogViewModel> Html { get; private set; }
    }
}
#pragma warning restore 1591