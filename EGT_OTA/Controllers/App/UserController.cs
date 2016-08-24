﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CommonTools;
using EGT_OTA.Helper;
using EGT_OTA.Models;

namespace EGT_OTA.Controllers.App
{
    /// <summary>
    /// 用户
    /// </summary>
    public class UserController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 列表
        /// </summary>
        public ActionResult List()
        {
            var pager = new Pager();
            string Name = ZNRequest.GetString("Name");
            var query = new SubSonic.Query.Select(Repository.GetProvider()).From<Music>();
            if (!string.IsNullOrWhiteSpace(Name))
            {
                query = query.And("Name").Like("%" + Name + "%");
            }
            var recordCount = query.GetRecordCount();
            var totalPage = recordCount % pager.Size == 0 ? recordCount / pager.Size : recordCount / pager.Size + 1; //计算总页数
            var list = query.Paged(pager.Index, pager.Size).OrderDesc("ID").ExecuteTypedList<Music>();
            var newlist = (from l in list
                           select new
                           {
                               ID = l.ID,
                               Name = l.Name,
                               FileUrl = l.FileUrl,
                               CreateDate = l.CreateDate.ToString("yyyy-MM-dd hh:mm:ss"),
                               Status = EnumBase.GetDescription(typeof(Enum_Status), l.Status)
                           }).ToList();
            var result = new
            {
                page = pager.Index,
                records = recordCount,
                total = totalPage,
                rows = newlist
            };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 登录
        /// </summary>
        [AllowAnyone]
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
        [AllowAnyone]
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
        [AllowAnyone]
        public ActionResult ChangeAvatar()
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
                var avatar = ZNRequest.GetString("Avatar").Trim();
                if (string.IsNullOrEmpty(avatar))
                {
                    return Json(new { result = result, message = "参数异常" }, JsonRequestBehavior.AllowGet);
                }
                user.Avatar = avatar;
                result = db.Update<User>(user) > 0;
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error(ex.Message, ex);
                message = ex.Message;
            }
            return Json(new { result = result, message = message }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 修改签名
        /// </summary>
        [AllowAnyone]
        public ActionResult ChangeSignature()
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
                var signature = ZNRequest.GetString("Signature").Trim();
                if (string.IsNullOrEmpty(signature))
                {
                    return Json(new { result = result, message = "参数异常" }, JsonRequestBehavior.AllowGet);
                }
                user.Signature = signature;
                result = db.Update<User>(user) > 0;
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error(ex.Message, ex);
                message = ex.Message;
            }
            return Json(new { result = result, message = message }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        [AllowAnyone]
        public ActionResult ChangePassword()
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

        /// <summary>
        /// 邮箱认证
        /// </summary>
        [AllowAnyone]
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
        [AllowAnyone]
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
                    message = "邮箱验证失败！<br />可能原因如下：<br />1、验证码过期<br />2、点击连接时网络连接失败<br />请从新发送验证请求";
                }
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
