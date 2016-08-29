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
    /// 点赞管理
    /// </summary>
    public class ZanController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Edit()
        {
            var id = ZNRequest.GetInt("id");
            Zan model = null;
            if (id > 0)
            {
                model = db.Single<Zan>(x => x.ID == id);
            }
            if (model == null)
            {
                model = new Zan();
            }
            return View(model);
        }

        /// <summary>
        /// 列表
        /// </summary>
        public ActionResult List()
        {
            var pager = new Pager();
            var query = new SubSonic.Query.Select(Repository.GetProvider()).From<Zan>();
            string Name = ZNRequest.GetString("Name");
            if (!string.IsNullOrWhiteSpace(Name))
            {
                var array = db.Find<User>(x => x.NickName.Contains(Name)).Select(x => x.ID).ToArray();
                if (array.Length > 0)
                {
                    query = query.And("CreateUserID").In(array);
                }
            }
            var recordCount = query.GetRecordCount();
            var totalPage = recordCount % pager.Size == 0 ? recordCount / pager.Size : recordCount / pager.Size + 1;
            var list = query.Paged(pager.Index, pager.Size).OrderDesc("ID").ExecuteTypedList<Zan>();
            var users = new SubSonic.Query.Select(Repository.GetProvider(), "ID", "NickName").From<User>().Where("ID").In(list.Select(x => x.CreateUserID).ToArray()).ExecuteTypedList<User>();
            var articles = new SubSonic.Query.Select(Repository.GetProvider(), "ID", "Title").From<Article>().Where("ID").In(list.Select(x => x.ArticleID).ToArray()).ExecuteTypedList<Article>();
            var newlist = (from l in list
                           select new
                           {
                               ID = l.ID,
                               NickName = users.Exists(x => x.ID == l.CreateUserID) ? users.FirstOrDefault(x => x.ID == l.CreateUserID).NickName : "",
                               ArticleID = articles.Exists(x => x.ID == l.ArticleID) ? articles.FirstOrDefault(x => x.ID == l.ArticleID).ID : 0,
                               Title = articles.Exists(x => x.ID == l.ArticleID) ? articles.FirstOrDefault(x => x.ID == l.ArticleID).Title : "",
                               CreateDate = l.CreateDate.ToString("yyyy-MM-dd hh:mm:ss"),
                               UpdateDate = l.UpdateDate.ToString("yyyy-MM-dd hh:mm:ss"),
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
            var articleID = ZNRequest.GetInt("ArticleID");
            if (articleID == 0)
            {
                return Json(new { result = false, message = "文章信息异常" }, JsonRequestBehavior.AllowGet);
            }
            Article article = new SubSonic.Query.Select(Repository.GetProvider(), "ID", "CreateUserID").From<Article>().Where<Article>(x => x.ID == articleID).ExecuteSingle<Article>();
            if (article == null)
            {
                return Json(new { result = false, message = "文章信息异常" }, JsonRequestBehavior.AllowGet);
            }
            Zan model = db.Single<Zan>(x => x.CreateUserID == user.ID && x.ArticleID == articleID);
            if (model == null)
            {
                model = new Zan();
            }
            else
            {
                return Json(new { result = false, message = "已点赞" }, JsonRequestBehavior.AllowGet);
            }
            model.ArticleID = articleID;
            model.ArticleUserID = article.CreateUserID;
            model.Status = Enum_Status.Approved;
            model.UpdateUserID = user.ID;
            model.UpdateDate = DateTime.Now;
            model.UpdateIP = Tools.GetClientIP;
            try
            {
                if (model.ID == 0)
                {
                    model.CreateDate = DateTime.Now;
                    model.CreateUserID = user.ID;
                    model.CreateIP = Tools.GetClientIP;
                    result = Tools.SafeInt(db.Add<Zan>(model)) > 0;

                    //修改文章点赞数
                    if (result)
                    {
                        article.Goods = article.Goods + 1;
                        db.Update<Article>(article);
                    }
                }
                else
                {
                    result = db.Update<Zan>(model) > 0;
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
            var model = db.Single<Zan>(x => x.ID == id);
            try
            {
                if (model != null)
                {
                    model.UpdateUserID = user.ID;
                    model.UpdateDate = DateTime.Now;
                    model.UpdateIP = Tools.GetClientIP;
                    model.Status = Enum_Status.DELETE;
                    result = db.Update<Zan>(model) > 0;
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
        /// 列表
        /// </summary>
        [AllowAnyone]
        public ActionResult All()
        {
            var pager = new Pager();
            var query = new SubSonic.Query.Select(Repository.GetProvider()).From<Zan>().Where<Zan>(x => x.Status == Enum_Status.Approved);
            var UserID = ZNRequest.GetInt("UserID");
            if (UserID > 0)
            {
                query = query.And("CreateUserID").IsEqualTo(UserID);
            }
            var recordCount = query.GetRecordCount();
            var totalPage = recordCount % pager.Size == 0 ? recordCount / pager.Size : recordCount / pager.Size + 1;
            var list = query.Paged(pager.Index, pager.Size).OrderDesc("ID").ExecuteTypedList<Zan>();
            var articles = new SubSonic.Query.Select(Repository.GetProvider(), "ID", "Title", "TypeID", "Cover", "Views", "Goods", "Keeps", "Comments", "CreateUserID", "CreateDate").From<Article>().Where("ID").In(list.Select(x => x.ArticleID).ToArray()).ExecuteTypedList<Article>();
            var articletypes = new SubSonic.Query.Select(Repository.GetProvider(), "ID", "Name").From<ArticleType>().ExecuteTypedList<ArticleType>();
            var users = new SubSonic.Query.Select(Repository.GetProvider(), "ID", "NickName", "Avatar", "Signature").From<User>().Where("ID").In(articles.Select(x => x.CreateUserID).ToArray()).ExecuteTypedList<User>();
            var newlist = (from a in articles
                           join u in users on a.CreateUserID equals u.ID
                           join t in articletypes on a.TypeID equals t.ID
                           select new
                           {
                               UserID = u.ID,
                               NickName = u.NickName,
                               Signature = u.Signature,
                               Avatar = GetFullUrl(u.Avatar),
                               ArticleID = a.ID,
                               Title = a.Title,
                               Cover = GetFullUrl(a.Cover),
                               Views = a.Views,
                               Goods = a.Goods,
                               Comments = a.Comments,
                               Keeps = a.Keeps,
                               CreateDate = a.CreateDate.ToString("yyyy-MM-dd"),
                               TypeaName = t.Name
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

        #endregion
    }
}