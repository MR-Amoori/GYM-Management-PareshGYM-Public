using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Paresh_GYM_Management_System
{
    public partial class frmLogin : Form
    {
        public frmLogin()
        {
            InitializeComponent();
        }

        private void frmLogin_Load(object sender, EventArgs e)
        {
            this.BackColor = Color.FromArgb(26, 35, 58);
            txtUsername.BackColor = Color.FromArgb(39, 44, 77);
            txtPassword.BackColor = Color.FromArgb(39, 44, 77);
        }

        private void tlsBtnProgrammer_Click(object sender, EventArgs e)
        {
            ProcessStartInfo sInfo = new ProcessStartInfo("https://mramoori.ir");
            Process.Start(sInfo);

        }

        private void txtUsername_Click(object sender, EventArgs e)
        {
            txtUsername.Text = "";
        }

        private void txtPassword_Click(object sender, EventArgs e)
        {
            txtPassword.Text = "";
            txtPassword.PasswordChar = '*';
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private bool LoadCredentials(out string username, out string password)
        {
            username = string.Empty;
            password = string.Empty;

            string credentialsPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "credentials.txt");

            if (!File.Exists(credentialsPath))
            {
                MessageBox.Show(
                    "فایل اطلاعات ورود پیدا نشد:\n\n" + credentialsPath,
                    "خطا",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return false;
            }

            try
            {
                string[] lines = File.ReadAllLines(
                    credentialsPath,
                    Encoding.UTF8);

                foreach (string rawLine in lines)
                {
                    string line = rawLine.Trim();

                    // خطوط خالی و توضیحات نادیده گرفته می‌شوند
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                        continue;

                    int separatorIndex = line.IndexOf('=');

                    // خط باید به شکل key=value باشد
                    if (separatorIndex <= 0)
                        continue;

                    string key = line
                        .Substring(0, separatorIndex)
                        .Trim()
                        .ToLowerInvariant();

                    string value = line
                        .Substring(separatorIndex + 1)
                        .Trim();

                    if (key == "username")
                    {
                        username = value;
                    }
                    else if (key == "password")
                    {
                        password = value;
                    }
                }

                if (string.IsNullOrWhiteSpace(username) ||
                    string.IsNullOrWhiteSpace(password))
                {
                    MessageBox.Show(
                        "اطلاعات ورود در فایل credentials.txt کامل نیست.\n\n" +
                        "ساختار صحیح فایل:\n" +
                        "username=نام_کاربری\n" +
                        "password=رمز_عبور",
                        "خطا",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "خطا در خواندن فایل اطلاعات ورود:\n\n" + ex.Message,
                    "خطا",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return false;
            }
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (!LoadCredentials(
                    out string validUsername,
                    out string validPassword))
            {
                return;
            }

            string enteredUsername = txtUsername.Text.Trim();
            string enteredPassword = txtPassword.Text;

            if (enteredUsername == validUsername &&
                enteredPassword == validPassword)
            {
                MessageBox.Show(
                    "ورود شما با موفقیت انجام گردید",
                    "ورود",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
            }
            else
            {
                MessageBox.Show(
                    "نام کاربری یا رمز عبور نادرست است",
                    "خطا",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                txtPassword.SelectAll();
                txtPassword.Focus();
            }
        }

        private void txtPassword_Enter(object sender, EventArgs e)
        {
            txtPassword.Text = "";
            txtPassword.PasswordChar = '*';
        }
    }
}
