using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using Common;

namespace Server.Controllers
{
    [Route("[controller]/[action]")]
    public class MessageController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        /*
         {
            QueueID:0,"From":1,"To":2,"Message":"Hello world"
            }
             */

        private static IList<User> _userLogins = new List<User>();
        private static IList<Message> _messages = new List<Message>();

        private static List<User> _users = new List<User>
        {
            new User{Id=0,UserName="������"},
            new User{Id=1,UserName="User01"},
            new User{Id=2,UserName="User02"},
            new User{Id=3,UserName="User03"},
            new User{Id=4,UserName="User04"},
            new User{Id=5,UserName="User05"},
            new User{Id=6,UserName="User06"},
            new User{Id=7,UserName="User07"},
            new User{Id=8,UserName="User08"}
        };

        [HttpPost]
        public IActionResult Login([FromBody] User user)
        {
            if (_users.FirstOrDefault(u => u.Id == user.Id) == null)
                return Json(new ServiceResult { Success = false, Message = "�û������ڣ���¼ʧ��" });
            if (null != _userLogins.FirstOrDefault(u => u.Id == user.Id))
                return Json(new ServiceResult { Success = false, Message = $"�û�{user.UserName}�ѱ������˵�¼����һ���˺�����" });
            _userLogins.Add(user);
            return Json(new ServiceResult { Success = true });
        }

        [HttpPost]
        public IActionResult GetUsers(bool isAll)
        {
            if (isAll)
                return Json(_users.Skip(1));
            var list = new List<User>(_userLogins);
            list.Insert(0, new User { Id = 0, UserName = "All logined user" });
            return Json(list);
        }

        [HttpPost]
        public IActionResult Logout(long userId)
        {
            var u = _userLogins.FirstOrDefault(us => us.Id == userId);
            if (u != null)
            {
                _userLogins.Remove(u);
                return Json(new ServiceResult { Success = true });
            }
            return Json(new ServiceResult { Success = false, Message = "�û������ڣ����ʷǷ�" });
        }

        [HttpPost]
        public IActionResult Send([FromBody] Message message)
        {
            if (message == null)
                return Content("");
            //var repMsg = _messages.Where(m => m.QueueID > message.QueueID && (m.From == message.To)).OrderBy(m => m.QueueID).FirstOrDefault();
            //if (repMsg != null)
            //    return Json(repMsg);
            lock (_messages)
            {
                _messages.Add(new Message
                {
                    QueueID = _messages.Count > 0 ? _messages.Max(q => q.QueueID) + 1 : 1,
                    From = message.From,
                    FromName = _users.FirstOrDefault(u => u.Id == message.From).UserName,
                    To = message.To,
                    ToName = _users.FirstOrDefault(u => u.Id == message.To).UserName,
                    Content = message.Content
                });
            }
            return Content("");
        }

        public IActionResult Receive(long userId, long queueId)
        {
            Message retMsg = null;
            int seconds = 0;
            do
            {
                lock (_messages)
                {
                    retMsg = _messages.Where(m => (m.To == userId || m.To == 0) && m.From != userId && queueId < m.QueueID).FirstOrDefault();
                }
                if (retMsg == null)
                {
                    Thread.Sleep(1000); // 5s
                    seconds += 1;
                }
            } while (retMsg == null && seconds < 30);
            return Json(retMsg);
        }
    }

    public class Message
    {
        // �ϴ���Ϣֵ���ͻ��˽��ϴη��������ص�ֵԭ�����أ�
        public long QueueID { get; set; }
        /// <summary>
        /// ������
        /// </summary>
        public long From { get; set; }
        public string FromName { get; set; }
        /// <summary>
        /// ������0Ϊȫ��
        /// </summary>
        public long To { get; set; }

        public string ToName { get; set; }
        /// <summary>
        /// ��Ϣ����
        /// </summary>
        public string Content { get; set; }
    }

    public class User
    {
        public long Id { get; set; }
        public string UserName { get; set; }
    }
}