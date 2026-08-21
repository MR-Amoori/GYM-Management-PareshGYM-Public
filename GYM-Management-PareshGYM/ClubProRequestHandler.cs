using CefSharp;
using CefSharp.Handler;
using System;

namespace GYM_Management_PareshGYM
{
    public class ClubProRequestHandler : RequestHandler
    {
        private readonly string panelUrl =
            "https://clubpro.ir/panel";

        protected override bool OnBeforeBrowse(
            IWebBrowser chromiumWebBrowser,
            IBrowser browser,
            IFrame frame,
            IRequest request,
            bool userGesture,
            bool isRedirect)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.Url))
            {
                return false;
            }

            if (!frame.IsMain)
            {
                return false;
            }

            if (IsBlockedUrl(request.Url))
            {
                browser.MainFrame.LoadUrl(panelUrl);

                // جلوگیری از بازشدن لینک اصلی
                return true;
            }

            return false;
        }

        private bool IsBlockedUrl(string url)
        {
            if (!Uri.TryCreate(
                    url,
                    UriKind.Absolute,
                    out Uri uri))
            {
                return false;
            }

            bool validHost =
                uri.Host.Equals(
                    "clubpro.ir",
                    StringComparison.OrdinalIgnoreCase)
                ||
                uri.Host.Equals(
                    "www.clubpro.ir",
                    StringComparison.OrdinalIgnoreCase);

            if (!validHost)
            {
                return false;
            }

            string path =
                uri.AbsolutePath
                    .TrimEnd('/')
                    .ToLowerInvariant();

            return path == "/help" ||
                   path == "/panel/plans" ||
                   path == "/logout" ||
                   path == "/support";
        }
    }
}