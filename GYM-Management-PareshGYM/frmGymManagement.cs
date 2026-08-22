using CefSharp;
using CefSharp.WinForms;
using Paresh_GYM_Management_System;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using CefSharp.Handler;
using System.Diagnostics;

namespace GYM_Management_PareshGYM
{
    public partial class frmGymManagement : Form
    {
        private ChromiumWebBrowser browser;
        private PictureBox loadingPictureBox;

        // بهتر است این اطلاعات در تنظیمات رمزنگاری‌شده ذخیره شوند.
        // یک کلاس با نام Passwords درست میشه با فرمت زیر:
        //internal readonly string clubProUsername = "09120000000";
        //internal readonly string clubProPassword = "kad2@kjs";
       // Passwords passwords = new Passwords();

        private bool phoneLoginSubmitted = false;
        private bool passwordLoginSubmitted = false;
        private bool loginStarted = false;



        public frmGymManagement()
        {
            InitializeComponent();

            FormBorderStyle = FormBorderStyle.FixedSingle;

            MaximizeBox = false;
            MinimizeBox = true;
            ControlBox = true;
            ShowInTaskbar = true;
            TopMost = false;

            // فرم از ابتدا کل صفحه را بگیرد.
            WindowState = FormWindowState.Maximized;

            splitContainer1.Panel2.Visible = true;
            splitContainer1.Panel2.Enabled = true;

            CreateLoadingOverlay();

            FormClosed += frmGymManagement_FormClosed;
        }

        protected override void WndProc(
            ref Message m)
        {
            const int WM_NCLBUTTONDOWN = 0x00A1;
            const int WM_NCLBUTTONDBLCLK = 0x00A3;
            const int WM_SYSCOMMAND = 0x0112;

            const int HTCAPTION = 0x0002;
            const int SC_MOVE = 0xF010;
            const int SC_MAXIMIZE = 0xF030;

            if ((m.Msg == WM_NCLBUTTONDOWN ||
                 m.Msg == WM_NCLBUTTONDBLCLK) &&
                m.WParam.ToInt32() == HTCAPTION)
            {
                return;
            }

            if (m.Msg == WM_SYSCOMMAND)
            {
                int command =
                    m.WParam.ToInt32() & 0xFFF0;

                if (command == SC_MOVE ||
                    command == SC_MAXIMIZE)
                {
                    return;
                }
            }

            base.WndProc(ref m);
        }

        private void CreateLoadingOverlay()
        {
            loadingPictureBox = new PictureBox();

            loadingPictureBox.Dock = DockStyle.Fill;

            loadingPictureBox.BackColor =
                Color.White;

            loadingPictureBox.SizeMode =
                PictureBoxSizeMode.Zoom;

            loadingPictureBox.Image =
                Properties.Resources.Logo_White;

            loadingPictureBox.Margin =
                new Padding(0);

            loadingPictureBox.Padding =
                new Padding(0);

            splitContainer1.Panel2.Controls.Add(
                loadingPictureBox);

            loadingPictureBox.BringToFront();
        }

        private void InitBrowser()
        {
            HideWebsite();

            if (browser != null &&
                !browser.IsDisposed)
            {
                return;
            }

            if (!Cef.IsInitialized)
            {
                CefSettings settings = new CefSettings();

                bool initialized =
                    Cef.Initialize(settings);

                if (!initialized)
                {
                    MessageBox.Show(
                        "سیستم داخلی برنامه راه‌اندازی نشد.",
                        "خطا",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    Close();
                    return;
                }
            }

            browser = new ChromiumWebBrowser(
                "about:blank");

            browser.RequestHandler =
    new ClubProRequestHandler();

            browser.DownloadHandler = new CustomDownloadHandler();

            browser.Dock = DockStyle.Fill;

            browser.FrameLoadEnd +=
                Browser_FrameLoadEnd;

            browser.IsBrowserInitializedChanged +=
                Browser_IsBrowserInitializedChanged;

            browser.LoadError +=
                Browser_LoadError;

            // مرورگر به‌عنوان لایه زیرین اضافه می‌شود.
            splitContainer1.Panel2.Controls.Add(
                browser);

            // PictureBox دوباره به بالاترین لایه منتقل می‌شود.
            loadingPictureBox.BringToFront();

            browser.CreateControl();
        }


        private void Browser_LoadError(
    object sender,
    LoadErrorEventArgs e)
        {
            if (e.ErrorCode == CefErrorCode.Aborted)
            {
                return;
            }

            if (IsDisposed)
            {
                return;
            }

            BeginInvoke(new Action(() =>
            {
                if (IsDisposed)
                {
                    return;
                }

                MessageBox.Show(
                    "صفحه بارگذاری نشد.\n\n" +
                    "خطا: " + e.ErrorText + "\n\n",
                    "خطای بارگذاری",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }));
        }

        private async void Browser_FrameLoadEnd(
            object sender,
            FrameLoadEndEventArgs e)
        {
            if (!e.Frame.IsMain)
            {
                return;
            }

            if (browser == null ||
                browser.IsDisposed)
            {
                return;
            }

            string currentUrl =
                e.Url ?? string.Empty;

            // اگر صفحه پنل باز شده باشد، سایت را نمایش بده.
            if (IsPanelPage(currentUrl))
            {
                ShowWebsite();

                return;
            }

            // در صفحه لاگین، سایت همچنان مخفی باقی می‌ماند.
            if (!IsLoginPage(currentUrl))
            {
                return;
            }

            HideWebsite();
            Debug.WriteLine($"تغییر صفحه لود شد: {currentUrl}");

            if (!browser.CanExecuteJavascriptInMainFrame)
            {
                await Task.Delay(500);

                if (browser == null ||
                    browser.IsDisposed ||
                    !browser.CanExecuteJavascriptInMainFrame)
                {
                    return;
                }
            }

            bool isPasswordStep =
                currentUrl.IndexOf(
                    "step=password",
                    StringComparison.OrdinalIgnoreCase) >= 0;

            if (isPasswordStep)
            {
                if (passwordLoginSubmitted)
                {
                    return;
                }

                await SubmitPasswordAsync();
            }
            else
            {
                if (phoneLoginSubmitted)
                {
                    return;
                }

                await SubmitPhoneAsync();
            }
        }

        private async Task SubmitPhoneAsync()
        {
            bool phoneExists =
                await WaitForElementAsync(
                    "#phoneNumber");

            if (!phoneExists)
            {
                ShowLoginError(
                    "فیلد شماره موبایل پیدا نشد.");

                return;
            }

            string encodedUsername =
                JavaScriptEncode(Passwords.SelectedWebUsername);

            string script = $@"
(function () {{
    var input =
        document.querySelector('#phoneNumber');

    if (!input) {{
        return 'PHONE_NOT_FOUND';
    }}

    input.focus();

    var setter =
        Object.getOwnPropertyDescriptor(
            HTMLInputElement.prototype,
            'value'
        ).set;

    setter.call(
        input,
        '{encodedUsername}'
    );

    input.dispatchEvent(
        new Event('input', {{ bubbles: true }})
    );

    input.dispatchEvent(
        new Event('change', {{ bubbles: true }})
    );

    var form =
        input.closest('form');

    if (!form) {{
        return 'FORM_NOT_FOUND';
    }}

    var button =
        form.querySelector(
            'button[type=""submit""], input[type=""submit""]'
        );

    if (!button) {{
        return 'BUTTON_NOT_FOUND';
    }}

    button.click();

    return 'PHONE_SUBMITTED';
}})();
";

            JavascriptResponse response =
                await browser.EvaluateScriptAsync(script);

            if (response.Success &&
                response.Result != null &&
                response.Result.ToString() ==
                "PHONE_SUBMITTED")
            {
                phoneLoginSubmitted = true;
            }
        }

        private async Task SubmitPasswordAsync()
        {

            Debug.WriteLine("شروع جستجوی فیلد رمز عبور...");
            bool passwordExists = await WaitForElementAsync("#loginPassword");
            if (!passwordExists)
            {
                Debug.WriteLine("تایم‌اوت: فیلد رمز عبور در صفحه پیدا نشد.");
                ShowLoginError("فیلد رمز عبور پیدا نشد.");
                return;
            }
            Debug.WriteLine("فیلد رمز عبور پیدا شد. ارسال اطلاعات...");

            string encodedPassword =
                JavaScriptEncode(Passwords.SelectedWebPassword);

            string script = $@"
(function () {{
    var input =
        document.querySelector('#loginPassword');

    if (!input) {{
        return 'PASSWORD_NOT_FOUND';
    }}

    input.focus();

    var setter =
        Object.getOwnPropertyDescriptor(
            HTMLInputElement.prototype,
            'value'
        ).set;

    setter.call(
        input,
        '{encodedPassword}'
    );

    input.dispatchEvent(
        new Event('input', {{ bubbles: true }})
    );

    input.dispatchEvent(
        new Event('change', {{ bubbles: true }})
    );

    var form =
        input.closest('form');

    if (!form) {{
        return 'FORM_NOT_FOUND';
    }}

    var button =
        form.querySelector(
            'button[type=""submit""], input[type=""submit""]'
        );

    if (!button) {{
        return 'BUTTON_NOT_FOUND';
    }}

    button.click();

    return 'PASSWORD_SUBMITTED';
}})();
";

            JavascriptResponse response =
                await browser.EvaluateScriptAsync(script);

            if (response.Success &&
                response.Result != null &&
                response.Result.ToString() ==
                "PASSWORD_SUBMITTED")
            {
                passwordLoginSubmitted = true;
            }
        }

        private async Task<bool> WaitForElementAsync(
            string selector,
            int timeoutMilliseconds = 20000)
        {
            int elapsed = 0;

            while (elapsed < timeoutMilliseconds)
            {
                if (browser == null ||
                    browser.IsDisposed ||
                    !browser.CanExecuteJavascriptInMainFrame)
                {
                    await Task.Delay(300);

                    elapsed += 300;

                    continue;
                }

                string encodedSelector =
                    JavaScriptEncode(selector);

                string script = $@"
(function () {{
    return document.querySelector(
        '{encodedSelector}'
    ) !== null;
}})();
";

                try
                {
                    JavascriptResponse response =
                        await browser.EvaluateScriptAsync(script);

                    if (response.Success &&
                        response.Result is bool exists &&
                        exists)
                    {
                        return true;
                    }
                }
                catch
                {
                    // در زمان آماده‌سازی صفحه، خطای موقت نادیده گرفته می‌شود.
                }

                await Task.Delay(400);

                elapsed += 400;
            }

            return false;
        }

        private bool IsLoginPage(string url)
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

            return uri.AbsolutePath
                .TrimEnd('/')
                .Equals(
                    "/login",
                    StringComparison.OrdinalIgnoreCase);
        }

        private bool IsPanelPage(string url)
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

            return path == "/panel" ||
                   path.StartsWith("/panel/");
        }

        private void HideWebsite()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(HideWebsite));
                return;
            }

            if (loadingPictureBox == null ||
                loadingPictureBox.IsDisposed)
            {
                return;
            }

            loadingPictureBox.Visible = true;
            loadingPictureBox.BringToFront();
        }

        private void ShowWebsite()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(ShowWebsite));
                return;
            }

            if (loadingPictureBox != null &&
                !loadingPictureBox.IsDisposed)
            {
                loadingPictureBox.Visible = false;
            }

            if (browser != null &&
                !browser.IsDisposed)
            {
                browser.Visible = true;
                browser.BringToFront();
                browser.Focus();
            }
        }

        private void ShowLoginError(string message)
        {
            if (IsDisposed)
            {
                return;
            }

            BeginInvoke(new Action(() =>
            {
                if (IsDisposed)
                {
                    return;
                }

                MessageBox.Show(
                    message,
                    "خطا در ورود خودکار",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }));
        }

        private string JavaScriptEncode(string value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            return value
                .Replace("\\", "\\\\")
                .Replace("'", "\\'")
                .Replace("\"", "\\\"")
                .Replace("\r", "\\r")
                .Replace("\n", "\\n");
        }

        private void btnLogout_Click(
            object sender,
            EventArgs e)
        {
            DialogResult result =
                MessageBox.Show(
                    "آیا می‌خواهید از برنامه خارج شوید؟",
                    "خروج",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
            {
                return;
            }

            Application.Exit();
        }

        private void btnReloadPage_Click(
            object sender,
            EventArgs e)
        {
            if (browser == null ||
                browser.IsDisposed)
            {
                return;
            }

            loginStarted = true;

            phoneLoginSubmitted = false;
            passwordLoginSubmitted = false;

            HideWebsite();

            browser.Load(
                "https://clubpro.ir/login");
        }

        private void frmGymManagement_FormClosed(
            object sender,
            FormClosedEventArgs e)
        {
            DisposeBrowser();
        }

        private void Browser_IsBrowserInitializedChanged(
    object sender,
    EventArgs e)
        {
            if (browser == null ||
                browser.IsDisposed ||
                !browser.IsBrowserInitialized)
            {
                return;
            }

            if (loginStarted)
            {
                return;
            }

            loginStarted = true;

            if (IsDisposed ||
                !IsHandleCreated)
            {
                return;
            }

            BeginInvoke(new Action(() =>
            {
                if (IsDisposed ||
                    browser == null ||
                    browser.IsDisposed)
                {
                    return;
                }

                browser.Load(
                    "https://clubpro.ir/login");
            }));
        }

        private void DisposeBrowser()
        {
            if (browser != null)
            {
                if (!browser.IsDisposed)
                {
                    browser.Dispose();
                }

                browser = null;
            }

            if (loadingPictureBox != null)
            {
                if (!loadingPictureBox.IsDisposed)
                {
                    loadingPictureBox.Dispose();
                }

                loadingPictureBox = null;
            }
        }

        private void frmGymManagement_Load(
            object sender,
            EventArgs e)
        {
            using (frmLogin loginForm = new frmLogin())
            {
                DialogResult loginResult =
                    loginForm.ShowDialog(this);

                if (loginResult != DialogResult.OK)
                {
                    Close();
                    return;
                }
            }

            InitBrowser();
        }

        private void btnDownloads_Click(object sender, EventArgs e)
        {
            string downloadPath = System.IO.Path.Combine(Application.StartupPath, "Downloads");

            if (System.IO.Directory.Exists(downloadPath))
            {
                System.Diagnostics.Process.Start("explorer.exe", downloadPath);
            }
            else
            {
                MessageBox.Show("پوشه گزارشات هنوز ایجاد نشده است.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}