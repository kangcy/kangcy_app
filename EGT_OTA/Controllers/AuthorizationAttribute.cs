using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EGT_OTA.Models;

namespace EGT_OTA.Controllers
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public class AuthorizationAttribute : FilterAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext == null)
            {
                throw new Exception("异常");
            }
            else
            {
                if (!filterContext.ActionDescriptor.IsDefined(typeof(AllowAnyoneAttribute), true) && !filterContext.ActionDescriptor.ControllerDescriptor.IsDefined(typeof(AllowAnyoneAttribute), true))
                {
                    CurrentUser CurrentUser = System.Web.HttpContext.Current.User as CurrentUser;
                    if (!CurrentUser.IsAuthenticated)
                    {
                        filterContext.Result = new RedirectResult("~/Home/Login");
                    }
                }
            }
        }
    }

    /// <summary>
    /// 这个特性，表示这个方法不需要验证用户登录状态
    /// </summary>
    public class AllowAnyoneAttribute : FilterAttribute
    {

    }
}