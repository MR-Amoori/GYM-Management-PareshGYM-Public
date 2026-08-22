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

            LoadCredentials();
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

        private void LoadCredentials()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "credentials.txt");
            if (!File.Exists(path)) return;

            foreach (string line in File.ReadAllLines(path))
            {
                if (!string.IsNullOrWhiteSpace(line) && line.Contains("="))
                {
                    string[] parts = line.Split('=');
                    appUsers[parts[0].Trim()] = parts[1].Trim();
                }
            }
        }


        private Dictionary<string, string> appUsers = new Dictionary<string, string>
{
    { "admin_user", "ahmad" }, { "admin_pass", "021021" },
    { "men_user", "admin2" }, { "men_pass", "020020" },
    { "women_user", "admin3" }, { "women_pass", "030030" }
};

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string user = txtUsername.Text.Trim();
            string pass = txtPassword.Text;

            if (user == appUsers["admin_user"] && pass == appUsers["admin_pass"]) // مدیر
            {
                GYM_Management_PareshGYM.Passwords.SelectedWebUsername = GYM_Management_PareshGYM.Passwords.ManagerWebUser;
                GYM_Management_PareshGYM.Passwords.SelectedWebPassword = GYM_Management_PareshGYM.Passwords.ManagerWebPass;
                MessageBox.Show("ورود شما بعنوان مدیر با موفقیت انجام گردید", "ورود", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
            }
            else if (user == appUsers["men_user"] && pass == appUsers["men_pass"]) // منشی آقایان
            {
                GYM_Management_PareshGYM.Passwords.SelectedWebUsername = GYM_Management_PareshGYM.Passwords.MenSecWebUser;
                GYM_Management_PareshGYM.Passwords.SelectedWebPassword = GYM_Management_PareshGYM.Passwords.MenSecWebPass;
                MessageBox.Show("ورود شما بعنوان منشی آقایان با موفقیت انجام گردید", "ورود", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;

            }
            else if (user == appUsers["women_user"] && pass == appUsers["women_pass"]) // منشی بانوان
            {
                GYM_Management_PareshGYM.Passwords.SelectedWebUsername = GYM_Management_PareshGYM.Passwords.WomenSecWebUser;
                GYM_Management_PareshGYM.Passwords.SelectedWebPassword = GYM_Management_PareshGYM.Passwords.WomenSecWebPass;
                MessageBox.Show("ورود شما بعنوان منشی بانوان با موفقیت انجام گردید", "ورود", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
            }
            else
            {
                MessageBox.Show("نام کاربری یا رمز عبور نادرست است", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtPassword.SelectAll();
                txtPassword.Focus();
                return;
            }
        }

        private void txtPassword_Enter(object sender, EventArgs e)
        {
            txtPassword.Text = "";
            txtPassword.PasswordChar = '*';
        }

        private void rdbtnsDelete_Click(object sender, EventArgs e)
        {
            if (rdbtnMan.Checked == true)
            {
                rdbtnMan.Checked = false;
                txtUsername.Enabled = true;
                txtUsername.Text = "نام کاربری را وارد نمایید";
            }
            if (rdbtnWoman.Checked == true)
            {
                rdbtnWoman.Checked = false;
                txtUsername.Enabled = true;
                txtUsername.Text = "نام کاربری را وارد نمایید";
            }
        }

        private void rdbtnMan_CheckedChanged(object sender, EventArgs e)
        {
            txtUsername.Text = "admin2";
            txtUsername.Enabled = false;
        }

        private void rdbtnWoman_CheckedChanged(object sender, EventArgs e)
        {
            txtUsername.Text = "admin3";
            txtUsername.Enabled = false;
        }
    }
}
