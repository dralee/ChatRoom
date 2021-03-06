﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinClient
{
    // CreatedBy:  Jackie Lee（天宇遊龍）
    // CreatedOn: 2017.07.24
    public class ServiceResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }
    public class User
    {
        public long Id { get; set; }
        public string UserName { get; set; }
    }

    public class Message
    {
        // 上次消息值（客户端将上次服务器返回的值原样返回）
        public long QueueID { get; set; }
        /// <summary>
        /// 发送者
        /// </summary>
        public long From { get; set; }
        public string FromName { get; set; }
        /// <summary>
        /// 接收者0为全部
        /// </summary>
        public long To { get; set; }

        public string ToName { get; set; }
        /// <summary>
        /// 消息内容
        /// </summary>
        public string Content { get; set; }
    }
}
