using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinClient.Manage;

namespace WinClient
{
    // CreatedBy:  Jackie Lee（天宇遊龍）
    // CreatedOn: 2017.07.24
    public partial class FormLogin : Form
    {
        public FormLogin()
        {
            InitializeComponent();

            Thread.Sleep(2000);
            Task.Factory.StartNew(() =>
            {
                var users = HttpManager.Instance.Users().Result;
                this.NeedInvoke(() =>
                {
                    cmbUsers.DataSource = users;
                    cmbUsers.DisplayMember = "UserName";
                    cmbUsers.ValueMember = "Id";
                });
            });
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            var item = cmbUsers.SelectedItem as User;
            ServiceResult result = null;
            Task.Factory.StartNew(() =>
            {
                result = HttpManager.Instance.Login(item.Id, item.UserName).Result;
                if (result.Success)
                {
                    FormMain fm = new FormMain(this);
                    this.NeedInvoke(() => Hide());
                    fm.ShowDialog();
                }
                else
                {
                    MessageBox.Show($"登录失败：\r\n{result.Message}");
                }
            });
        }
    }
}
