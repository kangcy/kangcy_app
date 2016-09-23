using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CommonTools;
using EGT_OTA.Helper;
using EGT_OTA.Models;
using Newtonsoft.Json;

namespace EGT_OTA.Controllers
{
    /// <summary>
    /// 用户
    /// </summary>
    public class UserController : BaseController
    {
        /// <summary>
        /// 登录
        /// </summary>
        public ActionResult Login()
        {
            var callback = ZNRequest.GetString("jsoncallback");
            try
            {
                var username = ZNRequest.GetString("UserName").Trim();
                var password = ZNRequest.GetString("Password").Trim();
                if (String.IsNullOrEmpty(username) || String.IsNullOrEmpty(password))
                {
                    return Content(callback + "(" + JsonConvert.SerializeObject(new { result = false, message = "用户名和密码不能为空" }) + ")");
                }
                password = DesEncryptHelper.Encrypt(password);
                User user = db.Single<User>(x => x.UserName == username && x.Password == password);
                if (user == null)
                {
                    return Content(callback + "(" + JsonConvert.SerializeObject(new { result = false, message = "用户名或密码错误" }) + ")");
                }
                else
                {
                    string info = "\r\n" + username + "于" + DateTime.Now.ToString() + "登录APP\r\n" + "登录IP为:" + Tools.GetClientIP;
                    LogHelper.UserLoger.Info(info);

                    user.LoginTimes += 1;
                    user.LastLoginDate = DateTime.Now;
                    user.LastLoginIP = Tools.GetClientIP;
                    var result = db.Update<User>(user) > 0;
                    if (result)
                    {
                        user.Address = user.ProvinceName + " " + user.CityName;
                        user.BirthdayText = user.Birthday.ToString("yyyy-MM-dd");
                        return Content(callback + "(" + JsonConvert.SerializeObject(new { result = true, message = user }) + ")");
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error(ex.Message, ex);
            }
            return Content(callback + "(" + JsonConvert.SerializeObject(new { result = false, message = "失败" }) + ")");
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
                user.NickName = SqlFilter(ZNRequest.GetString("NickName"));
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
        public ActionResult EditAvatar()
        {
            var callback = ZNRequest.GetString("jsoncallback");
            try
            {
                User user = GetUserInfo();
                if (user == null)
                {
                    return Content(callback + "(" + JsonConvert.SerializeObject(new { result = false, message = "用户信息验证失败" }) + ")");
                }
                var avatar = ZNRequest.GetString("Avatar").Trim();
                if (string.IsNullOrEmpty(avatar))
                {
                    return Content(callback + "(" + JsonConvert.SerializeObject(new { result = false, message = "请上传头像" }) + ")");
                }
                user.Avatar = avatar;
                var result = db.Update<User>(user) > 0;
                if (result)
                {
                    return Content(callback + "(" + JsonConvert.SerializeObject(new { result = true, message = "成功" }) + ")");
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error(ex.Message);
            }
            return Content(callback + "(" + JsonConvert.SerializeObject(new { result = false, message = "失败" }) + ")");
        }

        /// <summary>
        /// 修改地址
        /// </summary>
        public ActionResult EditAddress()
        {
            var callback = ZNRequest.GetString("jsoncallback");
            try
            {
                User user = GetUserInfo();
                if (user == null)
                {
                    return Content(callback + "(" + JsonConvert.SerializeObject(new { result = false, message = "用户信息验证失败" }) + ")");
                }
                user.ProvinceID = ZNRequest.GetInt("ProvinceID");
                user.CityID = ZNRequest.GetInt("CityID");
                user.ProvinceName = ZNRequest.GetString("ProvinceName");
                user.CityName = ZNRequest.GetString("CityName");
                var result = db.Update<User>(user) > 0;
                if (result)
                {
                    return Content(callback + "(" + JsonConvert.SerializeObject(new { result = true, message = "成功" }) + ")");
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error(ex.Message);
            }
            return Content(callback + "(" + JsonConvert.SerializeObject(new { result = false, message = "失败" }) + ")");
        }


        /// <summary>
        /// 修改性别
        /// </summary>
        public ActionResult EditSex()
        {
            var callback = ZNRequest.GetString("jsoncallback");
            try
            {
                User user = GetUserInfo();
                if (user == null)
                {
                    return Content(callback + "(" + JsonConvert.SerializeObject(new { result = false, message = "用户信息验证失败" }) + ")");
                }
                user.Sex = ZNRequest.GetInt("Sex");
                var result = db.Update<User>(user) > 0;
                if (result)
                {
                    return Content(callback + "(" + JsonConvert.SerializeObject(new { result = true, message = "成功" }) + ")");
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error(ex.Message);
            }
            return Content(callback + "(" + JsonConvert.SerializeObject(new { result = false, message = "失败" }) + ")");
        }

        /// <summary>
        /// 修改生日
        /// </summary>
        public ActionResult EditBirthday()
        {
            var callback = ZNRequest.GetString("jsoncallback");
            try
            {
                User user = GetUserInfo();
                if (user == null)
                {
                    return Content(callback + "(" + JsonConvert.SerializeObject(new { result = false, message = "用户信息验证失败" }) + ")");
                }
                user.Birthday = ZNRequest.GetDateTime("Birthday");
                var result = db.Update<User>(user) > 0;
                if (result)
                {
                    return Content(callback + "(" + JsonConvert.SerializeObject(new { result = true, message = "成功" }) + ")");
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error(ex.Message);
            }
            return Content(callback + "(" + JsonConvert.SerializeObject(new { result = false, message = "失败" }) + ")");
        }

        /// <summary>
        /// 修改昵称
        /// </summary>
        public ActionResult EditNickName()
        {
            var callback = ZNRequest.GetString("jsoncallback");
            try
            {
                User user = GetUserInfo();
                if (user == null)
                {
                    return Content(callback + "(" + JsonConvert.SerializeObject(new { result = false, message = "用户信息验证失败" }) + ")");
                }
                var NickName = SqlFilter(ZNRequest.GetString("NickName").Trim());
                if (string.IsNullOrEmpty(NickName))
                {
                    return Content(callback + "(" + JsonConvert.SerializeObject(new { result = false, message = "请填写昵称信息" }) + ")");
                }
                user.NickName = NickName;
                var result = db.Update<User>(user) > 0;
                if (result)
                {
                    return Content(callback + "(" + JsonConvert.SerializeObject(new { result = true, message = "成功" }) + ")");
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error(ex.Message);
            }
            return Content(callback + "(" + JsonConvert.SerializeObject(new { result = false, message = "失败" }) + ")");
        }

        /// <summary>
        /// 修改签名
        /// </summary>
        public ActionResult EditSignature()
        {
            var callback = ZNRequest.GetString("jsoncallback");
            try
            {
                User user = GetUserInfo();
                if (user == null)
                {
                    return Content(callback + "(" + JsonConvert.SerializeObject(new { result = false, message = "用户信息验证失败" }) + ")");
                }
                var Signature = SqlFilter(ZNRequest.GetString("Signature").Trim());
                if (string.IsNullOrEmpty(Signature))
                {
                    return Content(callback + "(" + JsonConvert.SerializeObject(new { result = false, message = "请填写签名信息" }) + ")");
                }
                user.Signature = Signature;
                var result = db.Update<User>(user) > 0;
                if (result)
                {
                    return Content(callback + "(" + JsonConvert.SerializeObject(new { result = true, message = "成功" }) + ")");
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error(ex.Message);
            }
            return Content(callback + "(" + JsonConvert.SerializeObject(new { result = false, message = "失败" }) + ")");
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        public ActionResult EditPassword()
        {
            var callback = ZNRequest.GetString("jsoncallback");
            try
            {
                User user = GetUserInfo();
                if (user == null)
                {
                    return Content(callback + "(" + JsonConvert.SerializeObject(new { result = false, message = "用户信息验证失败" }) + ")");
                }
                var newpassword = ZNRequest.GetString("NewPassword").Trim();
                if (string.IsNullOrEmpty(newpassword))
                {
                    return Content(callback + "(" + JsonConvert.SerializeObject(new { result = false, message = "参数异常" }) + ")");
                }
                newpassword = DesEncryptHelper.Encrypt(newpassword);
                if (user.Password == newpassword)
                {
                    return Content(callback + "(" + JsonConvert.SerializeObject(new { result = false, message = "新密码与原密码相同" }) + ")");
                }
                user.Password = newpassword;
                var result = db.Update<User>(user) > 0;
                if (result)
                {
                    return Content(callback + "(" + JsonConvert.SerializeObject(new { result = true, message = "成功" }) + ")");
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error(ex.Message);
            }
            return Content(callback + "(" + JsonConvert.SerializeObject(new { result = false, message = "失败" }) + ")");
        }

        /// <summary>
        /// 邮箱认证
        /// </summary>
        public ActionResult EmailVerify()
        {
            User user = GetUserInfo();
            if (user == null)
            {
                return Json(new { result = false, message = "用户信息验证失败" }, JsonRequestBehavior.AllowGet);
            }

            var result = false;
            var message = string.Empty;
            try
            {
                //判断是否已验证
                if (user.IsEmail == 1)
                {
                    return Json(new { result = result, message = "邮箱已认证" }, JsonRequestBehavior.AllowGet);
                }
                var email = ZNRequest.GetString("email");
                if (String.IsNullOrEmpty(email))
                {
                    return Json(new { result = result, message = "参数异常" }, JsonRequestBehavior.AllowGet);
                }
                //判断是否存在邮箱账号
                if (db.Exists<User>(x => x.Email == email && x.ID != user.ID))
                {
                    return Json(new { result = result, message = "该邮箱已被绑定" }, JsonRequestBehavior.AllowGet);
                }
                var code = Guid.NewGuid().ToString("N");
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
                MailHelper.SendMail("少侠网", body, fromUserModel);
                user.Email = email;
                result = db.Update<User>(user) > 0;
                return Json(new { result = result, message = "发送邮箱验证成功" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error(ex.Message, ex);
                message = ex.Message;
            }
            return Json(new { result = result, message = message }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 校验邮箱认证
        /// </summary>
        public ActionResult CheckEmail()
        {
            User user = GetUserInfo();
            if (user == null)
            {
                return Json(new { result = false, message = "用户信息验证失败" }, JsonRequestBehavior.AllowGet);
            }

            var result = false;
            var message = string.Empty;
            try
            {
                var uid = ZNRequest.GetInt("uid");
                var code = ZNRequest.GetString("code");
                var cookie = CookieHelper.GetCookieValue("email" + uid);
                if (code == cookie)
                {
                    if (user.IsEmail == 0)
                    {
                        user.IsEmail = 1;
                        result = db.Update<User>(user) > 0;
                        message = "邮箱验证成功！";
                    }
                    else
                    {
                        message = "邮箱已经验证,请勿重复验证";
                    }
                }
                else
                {
                    message = "邮箱验证失败！<br />可能原因如下：<br />1、验证码过期<br />2、点击连接时网络连接失败<br />请重新发送验证请求";
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error(ex.Message, ex);
                message = ex.Message;
            }
            return Json(new { result = result, message = message }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 修改设置
        /// </summary>
        public ActionResult ChangeSetting()
        {
            User user = GetUserInfo();
            if (user == null)
            {
                return Json(new { result = false, message = "用户信息验证失败" }, JsonRequestBehavior.AllowGet);
            }

            var result = false;
            var message = string.Empty;
            try
            {
                var newpassword = ZNRequest.GetString("newpassword").Trim();
                if (string.IsNullOrEmpty(newpassword))
                {
                    return Json(new { result = result, message = "参数异常" }, JsonRequestBehavior.AllowGet);
                }
                newpassword = DesEncryptHelper.Encrypt(newpassword);
                if (user.Password == newpassword)
                {
                    return Json(new { result = result, message = "新密码与原密码相同" }, JsonRequestBehavior.AllowGet);
                }
                user.Password = newpassword;
                result = db.Update<User>(user) > 0;

            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error(ex.Message, ex);
                message = ex.Message;
            }
            return Json(new { result = result, message = message }, JsonRequestBehavior.AllowGet);
        }
    }
}
