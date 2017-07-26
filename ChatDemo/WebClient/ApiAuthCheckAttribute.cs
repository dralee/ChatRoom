using Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebClient
{
    // CreatedBy:  Jackie Lee（天宇遊龍）
    // CreatedOn: 2017.07.24
    public class ApiAuthCheckAttribute : Attribute, IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {

        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (GetUser(context.HttpContext) == null)
            {
                context.Result = new JsonResult("Refresh");
            }
        }


        private User GetUser(HttpContext context)
        {
            byte[] buffer;
            if (!context.Session.TryGetValue("AuthUser", out buffer))
            {
                return null;
            }
            var user = buffer.FromBuffer().FromJson<User>();
            return user;
        }
    }
}
