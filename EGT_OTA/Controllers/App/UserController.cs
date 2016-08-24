using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CommonTools;
using EGT_OTA.Helper;
using EGT_OTA.Models;

namespace EGT_OTA.Controllers.App
{
    public class UserController : BaseController
    {
        /// <summary>
        /// 登录
        /// </summary>
        public ActionResult Login()
        {
            var status = false;
            var result = string.Empty;
            try
            {
                var username = ZNRequest.GetString("username").Trim();
                var password = ZNRequest.GetString("password").Trim();
                if (String.IsNullOrEmpty(username) || String.IsNullOrEmpty(password))
                {
                    return Json(new { status = status, result = "用户名和密码不能为空" }, JsonRequestBehavior.AllowGet);
                }
                password = DesEncryptHelper.Encrypt(password);
                User user = db.Single<User>(x => x.UserName == username && x.Password == password);
                if (user == null)
                {
                    return Json(new { result = "用户名或密码错误" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    string info = "\r\n" + username + "于" + DateTime.Now.ToString() + "登录APP\r\n" + "登录IP为:" + Tools.GetClientIP;
                    LogHelper.UserLoger.Info(info);

                    user.LoginTimes += 1;
                    user.LastLoginDate = DateTime.Now;
                    user.LastLoginIP = Tools.GetClientIP;
                    db.Update<User>(user);
                    status = true;
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error(ex.Message, ex);
                result = ex.Message;
            }
            return Json(new { status = status, result = result }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 注册
        /// </summary>
        public ActionResult Register()
        {
            var status = false;
            var result = string.Empty;
            try
            {
                var username = ZNRequest.GetString("username").Trim();
                var password = ZNRequest.GetString("password").Trim();
                if (String.IsNullOrEmpty(username) || String.IsNullOrEmpty(password))
                {
                    return Json(new { status = status, result = "用户名和密码不能为空" }, JsonRequestBehavior.AllowGet);
                }
                if (db.Exists<User>(x => x.UserName == username))
                {
                    return Json(new { status = status, result = "当前账号已注册" }, JsonRequestBehavior.AllowGet);
                }
                User user = new User();
                user.UserName = username;
                user.Password = DesEncryptHelper.Encrypt(password);
                user.Sex = ZNRequest.GetInt("sex", Enum_Sex.Boy);
                user.Email = string.Empty;
                user.Signature = string.Empty;
                user.Avatar = string.Empty;
                user.Phone = string.Empty;
                user.WeiXin = string.Empty;
                user.LoginTimes = 1;
                user.CreateDate = DateTime.Now;
                user.LastLoginDate = DateTime.Now;
                user.LastLoginIP = Tools.GetClientIP;
                user.IsPhone = 0;
                user.IsEmail = 0;
                user.Keeps = 0;
                user.Follows = 0;
                user.Fans = 0;
                status = Tools.SafeInt(db.Add<User>(user), 0) > 0 ? true : false;
                if (status)
                {
                    string info = "\r\n" + username + "于" + DateTime.Now.ToString() + "登录APP\r\n" + "登录IP为:" + Tools.GetClientIP;
                    LogHelper.UserLoger.Info(info);
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error(ex.Message, ex);
                result = ex.Message;
            }
            return Json(new { status = status, result = result }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 修改头像
        /// </summary>
        public ActionResult ChangeAvatar()
        {
            var status = false;
            var result = string.Empty;
            try
            {
                var id = ZNRequest.GetInt("id");
                var avatar = ZNRequest.GetString("avatar").Trim();
                if (id == 0 || string.IsNullOrEmpty(avatar))
                {
                    return Json(new { status = status, result = "参数异常" }, JsonRequestBehavior.AllowGet);
                }
                User user = db.Single<User>(x => x.ID == id);
                if (user == null)
                {
                    return Json(new { status = status, result = "用户信息异常" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    user.Avatar = avatar;
                    status = db.Update<User>(user) > 0;
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error(ex.Message, ex);
                result = ex.Message;
            }
            return Json(new { status = status, result = result }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 修改签名
        /// </summary>
        public ActionResult ChangeSignature()
        {
            var status = false;
            var result = string.Empty;
            try
            {
                var id = ZNRequest.GetInt("id");
                var signature = ZNRequest.GetString("signature").Trim();
                if (id == 0 || string.IsNullOrEmpty(signature))
                {
                    return Json(new { status = status, result = "参数异常" }, JsonRequestBehavior.AllowGet);
                }
                User user = db.Single<User>(x => x.ID == id);
                if (user == null)
                {
                    return Json(new { status = status, result = "用户信息异常" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    user.Signature = signature;
                    status = db.Update<User>(user) > 0;
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error(ex.Message, ex);
                result = ex.Message;
            }
            return Json(new { status = status, result = result }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        public ActionResult ChangePassword()
        {
            var status = false;
            var result = string.Empty;
            try
            {
                var username = ZNRequest.GetString("username").Trim();
                var password = ZNRequest.GetString("password").Trim();
                var newpassword = ZNRequest.GetString("newpassword").Trim();
                if (String.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(newpassword))
                {
                    return Json(new { status = status, result = "参数异常" }, JsonRequestBehavior.AllowGet);
                }
                if (password == newpassword)
                {
                    return Json(new { status = status, result = "新密码与原密码相同" }, JsonRequestBehavior.AllowGet);
                }
                User user = db.Single<User>(x => x.UserName == username && x.Password == password);
                if (user == null)
                {
                    return Json(new { status = status, result = "原密码不正确" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    user.Password = DesEncryptHelper.Encrypt(newpassword);
                    status = db.Update<User>(user) > 0;
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error(ex.Message, ex);
                result = ex.Message;
            }
            return Json(new { status = status, result = result }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 邮箱认证
        /// </summary>
        public ActionResult EmailVerify()
        {
            var status = false;
            var result = string.Empty;
            try
            {
                var username = ZNRequest.GetString("username").Trim();
                var password = ZNRequest.GetString("password").Trim();
                var email = ZNRequest.GetString("email");
                var code = Guid.NewGuid().ToString("N");
                if (String.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    return Json(new { status = status, result = "用户信息异常" }, JsonRequestBehavior.AllowGet);
                }
                User user = db.Single<User>(x => x.UserName == username && x.Password == password);
                if (user == null)
                {
                    return Json(new { status = status, result = "原密码不正确" }, JsonRequestBehavior.AllowGet);
                }
                //判断是否存在邮箱账号
                if (db.Exists<User>(x => x.Email == email && x.ID != user.ID))
                {
                    return Json(new { status = status, result = "该邮箱已被绑定" }, JsonRequestBehavior.AllowGet);
                }
                //判断是否已验证
                if (user.IsEmail == 1)
                {
                    return Json(new { status = status, result = "邮箱已认证" }, JsonRequestBehavior.AllowGet);
                }

                CookieHelper.SetCookie("email" + user.ID, code);

                var url = "http://localhost/app/User/CheckeEmail?uid=" + user.ID + "&code=" + code;
                string body = @"<strong>这是发给您的邮箱认证的邮件，有效期24小时</strong><p>此为系统邮件，请勿直接回复此邮件。</p> <br />
                                请点击下面的链接完成邮箱验证，如果链接无法转向，请复制一下链接到浏览器的地址栏中直接访问。 <br />
                               <a href='" + url + "' target='_blank'>请点击此处链接</a> <br />如果链接无法转向，请复制此连接" + url + "到浏览器的地址栏中直接访问<br />";
                FromUserModel fromUserModel = new FromUserModel
                {
                    UserID = "kangcy@axon.com.cn",
                    UserPwd = "YXhvbjEyMzQ=",
                    UserName = "少侠网",
                    ToUserArray = new ToUserModel[] { new ToUserModel { UserID = email, UserName = user.UserName } }
                };
                try
                {
                    MailHelper.SendMail("少侠网", body, fromUserModel);
                    user.Email = email;
                    status = db.Update<User>(user) > 0;
                    return Json(new { status = status, result = "发送邮箱验证成功" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    LogHelper.ErrorLoger.Error(ex.Message, ex);
                    result = ex.Message;
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error(ex.Message, ex);
                result = ex.Message;
            }
            return Json(new { status = status, result = result }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 校验邮箱认证
        /// </summary>
        public ActionResult CheckEmail()
        {
            var status = false;
            var result = string.Empty;
            try
            {
                var uid = ZNRequest.GetInt("uid");
                var code = ZNRequest.GetString("code");
                var cookie = CookieHelper.GetCookieValue("email" + uid);
                if (code == cookie)
                {
                    var user = db.Single<User>(x => x.ID == uid);
                    if (user != null)
                    {
                        if (user.IsEmail == 0)
                        {
                            user.IsEmail = 1;
                            status = db.Update<User>(user) > 0;
                            result = "邮箱验证成功！";
                        }
                        else
                        {
                            result = "邮箱已经验证,请勿重复验证";
                        }
                    }
                }
                else
                {
                    result = "邮箱验证失败！<br />可能原因如下：<br />1、验证码过期<br />2、点击连接时网络连接失败<br />请从新发送验证请求";
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error(ex.Message, ex);
                result = ex.Message;
            }
            return Json(new { status = status, result = result }, JsonRequestBehavior.AllowGet);
        }
    }
}
