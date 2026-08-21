using CefSharp;
using System.IO;
using System.Windows.Forms;

namespace GYM_Management_PareshGYM
{
    public class CustomDownloadHandler : IDownloadHandler
    {
        public bool CanDownload(IWebBrowser chromiumWebBrowser, IBrowser browser, string url, string requestMethod)
        {
            return true;
        }

        public void OnBeforeDownload(IWebBrowser chromiumWebBrowser, IBrowser browser, DownloadItem downloadItem, IBeforeDownloadCallback callback)
        {
            if (!callback.IsDisposed)
            {
                using (callback)
                {
                    // ساخت پوشه Downloads کنار فایل اجرایی برنامه
                    string downloadPath = Path.Combine(Application.StartupPath, "Downloads");
                    if (!Directory.Exists(downloadPath))
                    {
                        Directory.CreateDirectory(downloadPath);
                    }

                    // تعیین مسیر دقیق ذخیره فایل
                    string fullPath = Path.Combine(downloadPath, downloadItem.SuggestedFileName);

                    // دانلود بدون نمایش پنجره ذخیره
                    callback.Continue(fullPath, showDialog: false);
                }
            }
        }

        public void OnDownloadUpdated(IWebBrowser chromiumWebBrowser, IBrowser browser, DownloadItem downloadItem, IDownloadItemCallback callback)
        {
            if (downloadItem.IsComplete)
            {
                MessageBox.Show("دانلود فایل تکمیل شد.", "دانلود", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}