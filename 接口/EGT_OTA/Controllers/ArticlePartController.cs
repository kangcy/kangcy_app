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
    /// 文章部分管理
    /// </summary>
    public class ArticlePartController : BaseController
    {
        /// <summary>
        /// 编辑
        /// </summary>
        public ActionResult Edit()
        {
            try
            {
                User user = GetUserInfo();
                if (user == null)
                {
                    return Json(new { result = false, message = "用户信息验证失败" }, JsonRequestBehavior.AllowGet);
                }
                ArticlePart model = new ArticlePart();
                var id = ZNRequest.GetInt("ID");
                if (id > 0)
                {
                    model = db.Single<ArticlePart>(x => x.ID == id);
                }
                if (model == null)
                {
                    model = new ArticlePart();
                }
                model.ID = ZNRequest.GetInt("ID");
                if (model.ID == 0)
                {
                    model.ArticleID = ZNRequest.GetInt("ArticleID", 0);
                    model.Types = ZNRequest.GetInt("Types", 0);
                    if (model.ArticleID == 0)
                    {
                        return Json(new { result = false, message = "文章信息异常" }, JsonRequestBehavior.AllowGet);
                    }
                    if (model.Types == 0)
                    {
                        return Json(new { result = false, message = "段落信息异常" }, JsonRequestBehavior.AllowGet);
                    }
                    model.Index = 0;
                }
                model.Introduction = SqlFilter(ZNRequest.GetString("Introduction"), false);
                var newId = model.ID;
                var result = false;
                if (model.ID == 0)
                {
                    newId = Tools.SafeInt(db.Add<ArticlePart>(model));
                    result = newId > 0;
                }
                else
                {
                    result = db.Update<ArticlePart>(model) > 0;
                }
                return Json(new { result = true, message = newId }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error(ex.Message);
            }
            return Json(new { result = false, message = "失败" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 删除
        /// </summary>
        public ActionResult Delete()
        {
            try
            {
                User user = GetUserInfo();
                if (user == null)
                {
                    return Json(new { result = false, message = "用户信息验证失败" }, JsonRequestBehavior.AllowGet);
                }
                var id = ZNRequest.GetInt("ID");
                var model = db.Single<ArticlePart>(x => x.ID == id);
                if (model != null)
                {
                    var result = db.Delete<ArticlePart>(id) > 0;
                    if (result)
                    {
                        return Json(new { result = true, message = "成功" }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error(ex.Message);
            }
            return Json(new { result = false, message = "失败" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 列表
        /// </summary>
        public ActionResult All()
        {
            try
            {
                var pager = new Pager();
                var query = new SubSonic.Query.Select(Repository.GetProvider()).From<ArticlePart>();
                var ArticleID = ZNRequest.GetInt("ArticleID");
                if (ArticleID == 0)
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    query = query.Where<ArticlePart>(x => x.ArticleID == ArticleID);
                }
                var recordCount = query.GetRecordCount();
                var list = query.OrderDesc("ID").ExecuteTypedList<ArticlePart>();
                var result = new
                {
                    records = recordCount,
                    list = list
                };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error(ex.Message);
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
