﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EGT_OTA.Models;
using System.IO;
using Newtonsoft.Json;
using CommonTools;
using EGT_OTA.Helper;
using System.Web.Security;
using Newtonsoft.Json.Linq;
using System.Text;

namespace EGT_OTA.Controllers
{
    /// <summary>
    /// 意见反馈管理
    /// </summary>
    public class FeedBackController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Edit()
        {
            var id = ZNRequest.GetInt("id");
            FeedBack model = null;
            if (id > 0)
            {
                model = db.Single<FeedBack>(x => x.ID == id);
            }
            if (model == null)
            {
                model = new FeedBack();
            }
            return View(model);
        }

        /// <summary>
        /// 列表
        /// </summary>
        public ActionResult List()
        {
            var pager = new Pager();
            var query = new SubSonic.Query.Select(Repository.GetProvider()).From<FeedBack>();
            string Name = ZNRequest.GetString("Name");
            if (!string.IsNullOrWhiteSpace(Name))
            {
                var array = db.Find<User>(x => x.UserName == Name).Select(x => x.ID).ToArray();
                if (array.Length > 0)
                {
                    query = query.And("CreateUserID").In(array);
                }
            }
            var recordCount = query.GetRecordCount();
            var totalPage = recordCount % pager.Size == 0 ? recordCount / pager.Size : recordCount / pager.Size + 1; //计算总页数
            var list = query.Paged(pager.Index, pager.Size).OrderDesc("ID").ExecuteTypedList<FeedBack>();
            var users = new SubSonic.Query.Select(Repository.GetProvider(), "ID", "UserName").From<User>().And("ID").In(list.Select(x => x.CreateUserID).ToArray()).ExecuteTypedList<User>();
            var newlist = (from l in list
                           select new
                           {
                               ID = l.ID,
                               UserName = users.Exists(x => x.ID == l.CreateUserID) ? users.FirstOrDefault(x => x.ID == l.CreateUserID).UserName : "",
                               CreateDate = l.CreateDate.ToString("yyyy-MM-dd hh:mm:ss")
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

        #region  APP请求

        /// <summary>
        /// 编辑
        /// </summary>
        [AllowAnyone]
        public ActionResult Manage()
        {
            User user = GetUserInfo();
            if (user == null)
            {
                return Json(new { result = false, message = "用户信息验证失败" }, JsonRequestBehavior.AllowGet);
            }

            var result = false;
            var message = string.Empty;
            FeedBack model = new FeedBack();
            model.Summary = ZNRequest.GetString("UserID");
            try
            {
                model.CreateDate = DateTime.Now;
                model.CreateUserID = user.ID;
                model.CreateIP = Tools.GetClientIP;
                result = Tools.SafeInt(db.Add<FeedBack>(model)) > 0;
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error(ex.Message, ex);
                message = ex.Message;
            }
            return Json(new { result = result, message = message }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 删除
        /// </summary>
        [AllowAnyone]
        public ActionResult Delete()
        {
            User user = GetUserInfo();
            if (user == null)
            {
                return Json(new { result = false, message = "用户信息验证失败" }, JsonRequestBehavior.AllowGet);
            }

            var result = false;
            var message = string.Empty;
            var id = ZNRequest.GetInt("ids");
            try
            {
                if (id > 0)
                {
                    result = db.Delete<FeedBack>(id) > 0;
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error(ex.Message, ex);
                message = ex.Message;
            }
            return Json(new { result = result, message = message }, JsonRequestBehavior.AllowGet);
        }

        #endregion
    }
}