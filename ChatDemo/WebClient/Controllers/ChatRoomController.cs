using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using System.Text;
using Common;

namespace WebClient.Controllers
{
    // CreatedBy:  Jackie Lee（天宇遊龍）
    // CreatedOn: 2017.07.24
    public class ChatRoomController : Controller
    {
        public IActionResult Index()
        {
            var user = GetUser();
            if (user == null)
                return RedirectToAction("Login");
            ViewBag.UserName = user.UserName;
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(ViewUser user)
        {
            HttpClient client = new HttpClient();
            var u = new User { Id = user.Id, UserName = user.UserName };
            var content = new StringContent(u.ToJson());
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            var resp = await client.PostAsync("http://localhost:808/Message/Login", content);
            var res = await resp.Content.ReadAsStringAsync();
            var sr = res.FromJson<ServiceResult>();
            if (sr.Success)
            {
                HttpContext.Session.Set("AuthUser", u.ToJson().ToBuffer());
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", sr.Message);
            return View();
        }
        
        public async Task<IActionResult> Logout()
        {
            var user = GetUser();
            HttpClient client = new HttpClient();
            var content = new StringContent("");// new { userId = user.Id }.ToJson());
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            var resp = await client.PostAsync($"http://localhost:808/Message/Logout?userId={user.Id}", content);
            var res = await resp.Content.ReadAsStringAsync();
            var sr = res.FromJson<ServiceResult>();
            if (sr.Success)
            {
                HttpContext.Session.Remove("AuthUser");
                return RedirectToAction("Login");
            }
            ModelState.AddModelError("", sr.Message);
            return View("Index");
        }

        private User GetUser()
        {
            byte[] buffer;
            if (!HttpContext.Session.TryGetValue("AuthUser", out buffer))
            {
                return null;
            }
            var user = buffer.FromBuffer().FromJson<User>();
            return user;
        }

        private static long _queueID;

        [ApiAuthCheck]
        [HttpPost]
        //public async Task<IActionResult> Send(long from, long to, string msg)
        public async Task<IActionResult> Send(long to, string msg)
        {
            var user = GetUser();
            if (user == null)
                return Json("");
            var from = user.Id;

            HttpClient client = new HttpClient();
            //client.Timeout = new TimeSpan(300); // 30s
            var message = new Message
            {
                QueueID = _queueID,
                From = from,
                To = to,
                Content = msg
            };

            var content = new StringContent(message.ToJson());
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            var resp = await client.PostAsync("http://localhost:808/Message/Send", content);
            var res = await resp.Content.ReadAsStringAsync();
            if (res.IsNullOrEmpty())
                return Json("");
            var recMsg = res.FromJson<Message>();
            _queueID = recMsg.QueueID;
            return Json(recMsg);
        }

        [ApiAuthCheck]
        [HttpPost]
        //public IActionResult Receive(long userId)
        public IActionResult Receive()
        {
            var user = GetUser();
            if (user == null)
                return Json("Refresh");
            var userId = user.Id;

            HttpClient client = new HttpClient();
            var res = client.GetStringAsync($"http://localhost:808/Message/Receive?userId={userId}&queueId={_queueID}").Result;
            if (res.IsNullOrEmpty() || res.Equals("null"))
                return Json("");
            var recMsg = res.FromJson<Message>();
            _queueID = recMsg.QueueID;
            return Json(recMsg);
        }

        public async Task<IActionResult> Users()
        {
            var user = GetUser();
            HttpClient client = new HttpClient();
            var content = new StringContent("");
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            var resp = await client.PostAsync($"http://localhost:808/Message/GetUsers?isAll={(user == null)}", content);
            var userstr = await resp.Content.ReadAsStringAsync();
            var users = userstr.FromJson<List<User>>();

            if (user != null)
            {
                users.RemoveAll(u => u.Id == user.Id);
                //users.Insert(0, new User { Id = 0, UserName = "All logined user" });
            }
            return Json(users);
        }
    }

    public class ViewUser
    {
        public long Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }

    //public class Message
    //{
    //    // 上次消息值（客户端将上次服务器返回的值原样返回）
    //    public long QueueID { get; set; }
    //    /// <summary>
    //    /// 发送者
    //    /// </summary>
    //    public long From { get; set; }
    //    public string FromName { get; set; }
    //    /// <summary>
    //    /// 接收者0为全部
    //    /// </summary>
    //    public long To { get; set; }

    //    public string ToName { get; set; }
    //    /// <summary>
    //    /// 消息内容
    //    /// </summary>
    //    public string Content { get; set; }
    //}

    //public static class Extension
    //{
    //    public static string ToJson(this object obj)
    //    {
    //        return JsonConvert.SerializeObject(obj);
    //    }

    //    public static T FromJson<T>(this string str)
    //    {
    //        return JsonConvert.DeserializeObject<T>(str);
    //    }

    //    public static byte[] ToBuffer(this string str)
    //    {
    //        return Encoding.UTF8.GetBytes(str);
    //    }

    //    public static string FromBuffer(this byte[] buffer)
    //    {
    //        return Encoding.UTF8.GetString(buffer);
    //    }

    //    public static bool IsNullOrEmpty(this string str)
    //    {
    //        return string.IsNullOrEmpty(str);
    //    }
    //}
}