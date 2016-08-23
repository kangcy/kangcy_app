using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CommonTools;
using System.Web.Security;
using EGT_OTA.Models;

namespace EGT_OTA.HttpModules
{
    /// <summary>
    /// 身份验证处理管道
    /// </summary>
    public class AuthenModule : IHttpModule
    {
        static string[] unAthenRequest = { };

        static AuthenModule()
        {
            //不需要身份验证的请求
            //string str = "js.axd|css.axd|/admin/|.jpg|.png|.gif|.js|.css";
            string str = "js.axd|css.axd|.jpg|.png|.gif|.js|.css";
            unAthenRequest = str.Split('|');
        }

        public AuthenModule() { }

        public void Dispose() { }

        public void Init(HttpApplication application)
        {
            application.AuthenticateRequest += new EventHandler(AuthenticateRequest);
        }

        /// <summary>
        /// 验证用户是否登录
        /// </summary>
        private void AuthenticateRequest(object sender, EventArgs e)
        {
            HttpApplication app = (HttpApplication)sender;
            HttpContext context = (HttpContext)app.Context;

            foreach (string s in unAthenRequest)
            {
                if (context.Request.Path.ToLower().Contains(s))
                {
                    return;
                }
            }
            int currentUserID = 0;
            string authTicketText = null;//身份验证票
            //获取身份验证票
            HttpCookie luCookies = context.Request.Cookies[FormsAuthentication.FormsCookieName];
            if (luCookies != null)
            {
                authTicketText = luCookies.Value;
            }
            if (String.IsNullOrEmpty(authTicketText))
            {
                authTicketText = ZNRequest.GetString("token");
            }
            if (!String.IsNullOrEmpty(authTicketText))
            {
                try
                {
                    FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authTicketText);
                    FormsIdentity id = new FormsIdentity(authTicket);
                    string[] arrUserData = authTicket.UserData.Split('$');
                    if (arrUserData != null)
                    {
                        currentUserID = Tools.SafeInt(arrUserData[0]);
                        if (currentUserID != 0)
                        {
                            context.User = CurrentUser.GetCurrentUser(id, currentUserID);
                        }
                    }
                }
                catch
                {

                }
            }

            //用户未登陆
            if (String.IsNullOrEmpty(authTicketText) || context.User == null || !context.User.Identity.IsAuthenticated)
            {
                if (context.Request.Cookies[ConstKeys.COOKIEKEY_GUEST_USERID] != null)
                {
                    //若用户尚未登录且cookie中存在游客身份的用户ID,从cookie中取得用户的ID
                    currentUserID = Tools.SafeInt(context.Request.Cookies[ConstKeys.COOKIEKEY_GUEST_USERID].Value);
                }
                if (currentUserID == 0)
                {
                    currentUserID = ZNRequest.GetInt("GuestUserID");//从传递的参数中获取游客的UserID
                }
                context.User = CurrentUser.GetCurrentUser(new GuestIdentity(), currentUserID);
                if ((context.Request.Cookies[ConstKeys.COOKIEKEY_GUEST_USERID] == null
                    || String.IsNullOrEmpty(context.Request.Cookies[ConstKeys.COOKIEKEY_GUEST_USERID].Value)
                    || Tools.SafeInt(context.Request.Cookies[ConstKeys.COOKIEKEY_GUEST_USERID].Value) == 0)
                    && ZNRequest.GetInt("GuestUserID") == 0)//如果为浏览器发送的请求
                {
                    currentUserID = (context.User as CurrentUser).User.ID;
                    //将游客用户的UserID记录到cookie中
                    HttpCookie cookie = new HttpCookie(ConstKeys.COOKIEKEY_GUEST_USERID, currentUserID.ToString());
                    cookie.Expires = DateTime.MaxValue;
                    context.Response.Cookies.Add(cookie);
                }
            }
        }
    }
}