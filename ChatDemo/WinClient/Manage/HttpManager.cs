using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace WinClient.Manage
{
    // Jackie Lee 2017.07.24
    /// <summary>
    /// 管理
    /// </summary>
    public class HttpManager
    {
        private static long _queueId;
        private static User _user;

        private static HttpManager _instance = new HttpManager();
        public static HttpManager Instance
        {
            get { return _instance; }
        }

        public static User CurrentUser => _user;

        public async Task Send(long to, string msg)
        {
            HttpClient client = new HttpClient();
            var message = new Message
            {
                QueueID = _queueId,
                From = _user.Id,
                To = to,
                Content = msg
            };

            var content = new StringContent(message.ToJson());
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            var resp = await client.PostAsync("http://localhost:808/Message/Send", content);
            var res = await resp.Content.ReadAsStringAsync();
            if (res.IsNullOrEmpty())
                return;
        }

        public async Task<Message> Receive()
        {
            HttpClient client = new HttpClient();
            var res = await client.GetStringAsync($"http://localhost:808/Message/Receive?userId={_user.Id}&queueId={_queueId}");
            if (res.IsNullOrEmpty() || res.Equals("null"))
                return null;
            var recMsg = res.FromJson<Message>();
            _queueId = recMsg.QueueID;
            return recMsg;
        }

        public async Task<ServiceResult> Login(long id, string userName)
        {
            HttpClient client = new HttpClient();
            var u = new User { Id = id, UserName = userName };
            var content = new StringContent(u.ToJson());
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            var resp = await client.PostAsync("http://localhost:808/Message/Login", content);
            var res = await resp.Content.ReadAsStringAsync();
            var sResult = res.FromJson<ServiceResult>();
            if (sResult.Success)
            {
                _user = u;
            }
            return sResult;
        }

        public async Task<ServiceResult> Logout()
        {
            HttpClient client = new HttpClient();
            var content = new StringContent("");// new { userId = _user.Id }.ToJson());
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            var resp = await client.PostAsync($"http://localhost:808/Message/Logout?userId={_user.Id}", content);
            var res = await resp.Content.ReadAsStringAsync();
            var sResult = res.FromJson<ServiceResult>();
            if (sResult.Success)
            {
                _user = null;
            }
            return sResult;
        }

        public async Task<List<User>> Users()
        {
            HttpClient client = new HttpClient();
            var content = new StringContent("");
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            var resp = await client.PostAsync($"http://localhost:808/Message/GetUsers?isAll={(_user == null)}", content);
            var userstr = await resp.Content.ReadAsStringAsync();
            var users = userstr.FromJson<List<User>>();

            if (_user != null)
            {
                users.RemoveAll(u => u.Id == _user.Id);
                //users.Insert(0, new User { Id = 0, UserName = "All logined user" });
            }
            return users;
        }
    }
}
