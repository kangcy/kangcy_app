using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using EGT_OTA.Models;
using System.Text;
using CommonTools;
using EGT_OTA.Helper;
using System.Web.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EGT_OTA.Controllers
{
    public class HomeController : BaseController
    {
        /// <summary>
        /// 后台模板页
        /// </summary>
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 首页
        /// </summary>
        public ActionResult Default()
        {
            return View();
        }

        /// <summary>
        /// 登陆页
        /// </summary>
        [AllowAnyone]
        public ActionResult Login()
        {
            return View();
        }

        /// <summary>
        /// 管理员登陆
        /// </summary>
        [AllowAnyone]
        public ActionResult AdminLogin()
        {
            JsonObject message = new JsonObject();
            HttpContext context = System.Web.HttpContext.Current;
            var loginName = ZNRequest.GetString("LoginName").Trim();
            var password = ZNRequest.GetString("Password").Trim();
            if (String.IsNullOrEmpty(loginName) || String.IsNullOrEmpty(password))
            {
                return Json(new { status = false, result = "用户名和密码不能为空" }, JsonRequestBehavior.AllowGet);
            }
            var result = "0";
            try
            {
                UserInfo user = null;

                //系统默认管理员
                if (loginName == Admin_Name && password == Admin_Password)
                {
                    user = new UserInfo();
                    user.ID = -1;
                    user.UserName = loginName;
                    user.Password = password;
                    user.RoleID = -1;
                }
                else
                {
                    password = DesEncryptHelper.Encrypt(password);
                    user = db.Single<UserInfo>(x => x.UserName == loginName && x.Password == password);
                }
                if (user == null)
                {
                    return Json(new { status = false, result = "用户名或密码错误" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    context.Session[ConstKeys.SESSIONKEY_ADMIN_USERID] = user.ID.ToString();//保存Session
                    var userData = user.ID.ToString() + "$" + Tools.GetClientIP;
                    FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket(1, loginName, DateTime.Now, DateTime.Now.AddHours(8), true, userData);
                    var encryptedTicket = FormsAuthentication.Encrypt(authTicket);
                    HttpCookie userCookies = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
                    if (ZNRequest.GetBoolean("RememberMe"))
                    {
                        userCookies.Expires = DateTime.Now.AddDays(30);//记住用户登录状态30天
                    }
                    System.Web.HttpContext.Current.Response.Cookies.Add(userCookies);
                    result = context.Session[ConstKeys.SESSIONKEY_ADMIN_USERID].ToString();

                    //写登录日志
                    string info = "\r\n" + loginName + "于" + DateTime.Now.ToString() + "登录后台\r\n" + "登录IP为:" + Tools.GetClientIP;
                    LogHelper.UserLoger.Info(info);

                    if (loginName != Admin_Name || password != Admin_Password)
                    {
                        user.LoginTimes += 1;
                        user.LastLoginDate = DateTime.Now;
                        user.LastLoginIP = Tools.GetClientIP;
                        db.Update<UserInfo>(user);
                    }

                    return Json(new { status = true, result = "登录成功" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error(ex.Message, ex);
                result = ex.Message;
            }
            return Json(new { status = false, result = result }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 退出登陆
        /// </summary>
        public ActionResult LoginOut()
        {
            HttpContext context = System.Web.HttpContext.Current;
            context.Session.Remove(ConstKeys.SESSIONKEY_GUEST_USERID);
            context.Session.Remove(ConstKeys.SESSIONKEY_ADMIN_USERID);
            HttpCookie luCookies = context.Request.Cookies[FormsAuthentication.FormsCookieName];
            if (luCookies != null)
            {
                luCookies.Expires = DateTime.Now.AddDays(-1);
                context.Response.Cookies.Add(luCookies);
            }
            CurrentUser.RemoveCache();
            return Json(new { status = true }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 后台管理菜单
        /// </summary>
        public ActionResult Menu()
        {
            var str = MenuStr();
            var user = CurrentUser.User;

            List<AdminMenu> list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<AdminMenu>>(str);

            if (user.UserName == Admin_Name)
            {
                return Content(Newtonsoft.Json.JsonConvert.SerializeObject(list));
            }

            List<AdminMenu> newlist = new List<AdminMenu>();

            if (string.IsNullOrEmpty(user.Power))
            {
                newlist = new List<AdminMenu>();
            }
            else
            {
                //判断用户权限
                List<string> powerList = new List<string>();
                string[] powers = user.Power.Split(',');
                for (int i = 0; i < powers.Length; i++)
                {
                    if (!powers[i].Contains("_"))
                    {
                        powerList.Add(powers[i]);
                    }
                }
                list.ForEach(x =>
                {
                    if (powerList.Contains(x.ModuleId))
                    {
                        newlist.Add(x);
                    }
                });
            }
            str = Newtonsoft.Json.JsonConvert.SerializeObject(newlist);
            return Content(str);
        }

        /// <summary>
        /// 后台管理按钮
        /// </summary>
        public ActionResult Button()
        {
            var ModuleId = ZNRequest.GetString("ModuleId");
            var str = string.Empty;
            var button = string.Empty;
            if (!string.IsNullOrEmpty(ModuleId))
            {
                string menustr = MenuStr();
                List<AdminMenu> menulist = Newtonsoft.Json.JsonConvert.DeserializeObject<List<AdminMenu>>(menustr);
                AdminMenu menu = menulist.FirstOrDefault(x => x.ModuleId == ModuleId);
                if (!string.IsNullOrEmpty(menu.Button))
                {
                    button = menu.Button;
                }
            }
            if (!string.IsNullOrEmpty(button))
            {
                str = ButtonStr();
                var user = CurrentUser.User;
                List<Button> list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Button>>(str);
                if (user.UserName == Admin_Name)
                {
                    str = ButtonNoPower(ModuleId, list, button);
                }
                else
                {
                    str = ButtonHasPower(ModuleId, list, user);
                }
            }
            return Content(str);
        }

        /// <summary>
        /// 后台管理按钮（有权限限制）
        /// </summary>
        protected string ButtonHasPower(string moduleId, List<Button> list, UserInfo user)
        {
            string str = string.Empty;
            if (string.IsNullOrEmpty(user.Power))
            {
                str = string.Empty;
            }
            else
            {
                List<string> powerList = new List<string>();
                string[] powers = user.Power.Split(',');
                for (int i = 0; i < powers.Length; i++)
                {
                    if (powers[i].StartsWith(moduleId + "_"))
                    {
                        powerList.Add(powers[i].Replace(moduleId + "_", ""));
                    }
                }
                List<Button> newlist = new List<Button>();

                list.ForEach(x =>
                {
                    if (powerList.Contains(x.ButtonId))
                    {
                        newlist.Add(x);
                    }
                });
                str = Newtonsoft.Json.JsonConvert.SerializeObject(newlist);
            }
            return str;
        }

        /// <summary>
        /// 后台管理按钮（无权限限制）
        /// </summary>
        protected string ButtonNoPower(string moduleId, List<Button> list, string button)
        {
            var newlist = new List<Button>();
            list.ForEach(x =>
            {
                if (button.Contains("," + x.FullName + ","))
                {
                    newlist.Add(x);
                }
            });
            return Newtonsoft.Json.JsonConvert.SerializeObject(newlist);
        }
    }
}
