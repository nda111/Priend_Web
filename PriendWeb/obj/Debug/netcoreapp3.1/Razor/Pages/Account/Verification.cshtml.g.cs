#pragma checksum "C:\Users\nda11\source\Git\Priend\Priend_Web\PriendWeb\Pages\Account\Verification.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "c9601f256bb6cb53676d8157719133b55dd05fae"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(PriendWeb.Pages.Account.Pages_Account_Verification), @"mvc.1.0.razor-page", @"/Pages/Account/Verification.cshtml")]
namespace PriendWeb.Pages.Account
{
    #line hidden
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
#nullable restore
#line 1 "C:\Users\nda11\source\Git\Priend\Priend_Web\PriendWeb\Pages\_ViewImports.cshtml"
using PriendWeb;

#line default
#line hidden
#nullable disable
#nullable restore
#line 7 "C:\Users\nda11\source\Git\Priend\Priend_Web\PriendWeb\Pages\Account\Verification.cshtml"
using PriendWeb.Interaction;

#line default
#line hidden
#nullable disable
#nullable restore
#line 8 "C:\Users\nda11\source\Git\Priend\Priend_Web\PriendWeb\Pages\Account\Verification.cshtml"
using PriendWeb.Interaction.Membership.Web;

#line default
#line hidden
#nullable disable
#nullable restore
#line 9 "C:\Users\nda11\source\Git\Priend\Priend_Web\PriendWeb\Pages\Account\Verification.cshtml"
using System.Net.WebSockets;

#line default
#line hidden
#nullable disable
#nullable restore
#line 10 "C:\Users\nda11\source\Git\Priend\Priend_Web\PriendWeb\Pages\Account\Verification.cshtml"
using System.Threading;

#line default
#line hidden
#nullable disable
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemMetadataAttribute("RouteTemplate", "{hash}")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"c9601f256bb6cb53676d8157719133b55dd05fae", @"/Pages/Account/Verification.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"b5b216c2791a5adb5b3ef6f5743bfabc048234bc", @"/Pages/_ViewImports.cshtml")]
    public class Pages_Account_Verification : global::Microsoft.AspNetCore.Mvc.RazorPages.Page
    {
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
#nullable restore
#line 3 "C:\Users\nda11\source\Git\Priend\Priend_Web\PriendWeb\Pages\Account\Verification.cshtml"
  
    ViewData["Title"] = "Priend-Verification";

#line default
#line hidden
#nullable disable
            WriteLiteral("\r\n");
            WriteLiteral("\r\n");
#nullable restore
#line 12 "C:\Users\nda11\source\Git\Priend\Priend_Web\PriendWeb\Pages\Account\Verification.cshtml"
  
    string uri = $"wss://{Request.Host}/ws/membership/web/verify";

    var socket = new ClientWebSocket();
    await socket.ConnectAsync(new Uri(uri), CancellationToken.None);
    var conn = new WebSocketConnection(socket);

    await conn.SendTextAsync(Model.Hash);

    await conn.ReceiveAsync();
    VerificationResponse.EResponse response = (VerificationResponse.EResponse)conn.BinaryMessage[0];

    switch (response)
    {
        case VerificationResponse.EResponse.Ok:
            await conn.ReceiveAsync();
            string name = conn.TextMessage;


#line default
#line hidden
#nullable disable
            WriteLiteral("            <h1>Email Verified</h1>\r\n            <p>\r\n                Hello ");
#nullable restore
#line 32 "C:\Users\nda11\source\Git\Priend\Priend_Web\PriendWeb\Pages\Account\Verification.cshtml"
                 Write(name);

#line default
#line hidden
#nullable disable
            WriteLiteral(", <br /> <br />\r\n                Now you are ready to start Priend.<br />\r\n                I hope this would help you and your family! <br /> <br />\r\n                The Priend Team.\r\n            </p>\r\n");
#nullable restore
#line 37 "C:\Users\nda11\source\Git\Priend\Priend_Web\PriendWeb\Pages\Account\Verification.cshtml"
            break;

        case VerificationResponse.EResponse.UnknownHash:

#line default
#line hidden
#nullable disable
            WriteLiteral("            <h1>Unknown Page</h1>\r\n");
#nullable restore
#line 41 "C:\Users\nda11\source\Git\Priend\Priend_Web\PriendWeb\Pages\Account\Verification.cshtml"
            break;

        case VerificationResponse.EResponse.ServerError:
        default:

#line default
#line hidden
#nullable disable
            WriteLiteral("            <h1>Server Error</h1>\r\n");
#nullable restore
#line 46 "C:\Users\nda11\source\Git\Priend\Priend_Web\PriendWeb\Pages\Account\Verification.cshtml"
            break;
    }

    await conn.CloseAsync();

#line default
#line hidden
#nullable disable
            WriteLiteral("\r\n");
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
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<PriendWeb.Pages.Account.VerificationModel> Html { get; private set; }
        public global::Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary<PriendWeb.Pages.Account.VerificationModel> ViewData => (global::Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary<PriendWeb.Pages.Account.VerificationModel>)PageContext?.ViewData;
        public PriendWeb.Pages.Account.VerificationModel Model => ViewData.Model;
    }
}
#pragma warning restore 1591
