using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinClient.Manage;

namespace WinClient
{
    public partial class FormMain : Form
    {
        private FormLogin _formLogin;
        public FormMain(FormLogin formLogin)
        {
            _formLogin = formLogin;
            InitializeComponent();
            Disposed += FormMain_Disposed;
            FormClosed += FormMain_FormClosed;

            Task.Factory.StartNew(() =>
            {
                var users = HttpManager.Instance.Users().Result;
                this.NeedInvoke(() =>
                {
                    cmbTo.DataSource = users;
                    cmbTo.DisplayMember = "UserName";
                    cmbTo.ValueMember = "Id";
                });
            });

            lblLoginUser.Text = HttpManager.CurrentUser.UserName;

            _task = Task.Factory.StartNew(() =>
            {
                Receive();
            });
        }

        private void FormMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (_task != null)
                _task.Dispose();
        }

        private void FormMain_Disposed(object sender, EventArgs e)
        {
            if (_task != null)
                _task.Dispose();
        }

        Task _task;

        private void btnSend_Click(object sender, EventArgs e)
        {
            var from = HttpManager.CurrentUser.Id; //cmbUser.SelectedValue.ToLong();
            var to = cmbTo.SelectedValue.ToLong();
            var msg = txtMsg.Text.Trim();
            HttpManager.Instance.Send(to, msg);
            lbMsg.Items.Add($"[{HttpManager.CurrentUser.UserName}] to [{cmbTo.Text}] : {msg}");
            //lbMsg.Items.Add($"[{cmbUser.Text}] from [{cmbTo.Text}] : {retMsg.Content}");
        }

        private void Receive()
        {
            var retMsg = HttpManager.Instance.Receive().Result;
            if (retMsg != null)
            {
                this.NeedInvoke(() => lbMsg.Items.Add($"({retMsg.QueueID})[{ retMsg.FromName}] to [{ retMsg.ToName}] : { retMsg.Content}"));
                Receive();
            }
            else
            {
                Receive();
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                var users = HttpManager.Instance.Users().Result;
                this.NeedInvoke(() =>
                {
                    //cmbUser.DataSource = users;
                    //cmbUser.DisplayMember = "UserName";
                    //cmbUser.ValueMember = "Id";

                    //List<User> tos = new List<User>();
                    //users.ForEach(u =>
                    //{
                    //    tos.Add(u);
                    //});
                    cmbTo.DataSource = users;
                    cmbTo.DisplayMember = "UserName";
                    cmbTo.ValueMember = "Id";
                });
            });
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            Task.Factory.StartNew(async () =>
            {
                var res = await HttpManager.Instance.Logout();
                if (res.Success)
                {
                    _task.Dispose();
                    this.NeedInvoke(() =>
                    {
                        Close();
                        _formLogin.NeedInvoke(() => _formLogin.Show());
                    });
                }
                else
                {
                    MessageBox.Show(res.Message);
                }
            });
        }
    }
}
