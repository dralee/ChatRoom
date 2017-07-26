using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinClient
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

        public static long ToLong(this object obj)
        {
            return Convert.ToInt64(obj);
        }

        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }


        public static void NeedInvoke(this Form form, Action action)
        {
            if (form.InvokeRequired)
                form.BeginInvoke(new MethodInvoker(delegate
                {
                    action();
                }));
            else
                action();
        }
    }
}
