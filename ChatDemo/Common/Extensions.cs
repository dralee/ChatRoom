using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common
{
    public static class Extension
    {
        public static string ToJson(this object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public static T FromJson<T>(this string str)
        {
            return JsonConvert.DeserializeObject<T>(str);
        }

        public static byte[] ToBuffer(this string str)
        {
            return Encoding.UTF8.GetBytes(str);
        }

        public static string FromBuffer(this byte[] buffer)
        {
            return Encoding.UTF8.GetString(buffer);
        }

        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }
        public static long ToLong(this object obj)
        {
            return Convert.ToInt64(obj);
        }
    }
}
