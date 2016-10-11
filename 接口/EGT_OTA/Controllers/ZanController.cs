using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EGT_OTA.Models;
using System.IO;
using CommonTools;
using EGT_OTA.Helper;
using System.Web.Security;
using System.Text;
using Newtonsoft.Json;

namespace EGT_OTA.Controllers
{
    /// <summary>
    /// 点赞管理
    /// </summary>
    public class ZanController : BaseController
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
                var articleID = ZNRequest.GetInt("ArticleID");
                if (articleID == 0)
                {
                    return Json(new { result = false, message = "文章信息异常" }, JsonRequestBehavior.AllowGet);
                }
                Article article = new SubSonic.Query.Select(Repository.GetProvider(), "ID", "CreateUserID", "Goods").From<Article>().Where<Article>(x => x.ID == articleID).ExecuteSingle<Article>();
                if (article == null)
                {
                    return Json(new { result = false, message = "文章信息异常" }, JsonRequestBehavior.AllowGet);
                }
                Zan model = db.Single<Zan>(x => x.CreateUserID == user.ID && x.ArticleID == articleID && x.Status == Enum_Status.Approved);
                if (model == null)
                {
                    model = new Zan();
                    model.CreateDate = DateTime.Now;
                    model.CreateUserID = user.ID;
                    model.CreateIP = Tools.GetClientIP;
                }
                else
                {
                    return Json(new { result = false, message = "已赞" }, JsonRequestBehavior.AllowGet);
                }
                model.ArticleID = articleID;
                model.ArticleUserID = article.CreateUserID;
                model.Status = Enum_Status.Approved;
                model.UpdateUserID = user.ID;
                model.UpdateDate = DateTime.Now;
                model.UpdateIP = Tools.GetClientIP;
                var result = false;
                if (model.ID == 0)
                {
                    result = Tools.SafeInt(db.Add<Zan>(model)) > 0;
                }
                else
                {
                    result = db.Update<Zan>(model) > 0;
                }
                //修改点赞数
                if (result)
                {
                    var goods = article.Goods + 1;
                    result = new SubSonic.Query.Update<Article>(Repository.GetProvider()).Set("Goods").EqualTo(goods).Where<Article>(x => x.ID == articleID).Execute() > 0;
                    if (result)
                    {
                        return Json(new { result = true, message = goods }, JsonRequestBehavior.AllowGet);
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
        /// 删除
        /// </summary>
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

                    //修改文章点赞数
                    if (result)
                    {
                        Article article = new SubSonic.Query.Select(Repository.GetProvider(), "Goods").From<Article>().Where<Article>(x => x.ID == model.ArticleID).ExecuteSingle<Article>();
                        if (article != null && article.Goods > 0)
                        {
                            new SubSonic.Query.Update<Article>(Repository.GetProvider()).Set("Goods").EqualTo(article.Goods - 1).Where<Article>(x => x.ID == model.ArticleID).Execute();
                        }
                    }
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
        public ActionResult All()
        {
            try
            {
                var pager = new Pager();
                var query = new SubSonic.Query.Select(Repository.GetProvider()).From<Zan>().Where<Zan>(x => x.Status == Enum_Status.Approved);
                var CreateUserID = ZNRequest.GetInt("CreateUserID");
                if (CreateUserID > 0)
                {
                    query = query.And("CreateUserID").IsEqualTo(CreateUserID);
                }
                var ArticleUserID = ZNRequest.GetInt("ArticleUserID");
                if (ArticleUserID > 0)
                {
                    query = query.And("ArticleUserID").IsEqualTo(ArticleUserID);
                }
                var recordCount = query.GetRecordCount();
                var totalPage = recordCount % pager.Size == 0 ? recordCount / pager.Size : recordCount / pager.Size + 1;
                var list = query.Paged(pager.Index, pager.Size).OrderDesc("ID").ExecuteTypedList<Zan>();
                var articles = new SubSonic.Query.Select(Repository.GetProvider(), "ID", "Title", "TypeID", "Cover", "Views", "Goods", "Keeps", "Comments", "CreateUserID", "CreateDate").From<Article>().Where("ID").In(list.Select(x => x.ArticleID).ToArray()).ExecuteTypedList<Article>();
                var articletypes = new SubSonic.Query.Select(Repository.GetProvider(), "ID", "Name").From<ArticleType>().ExecuteTypedList<ArticleType>();
                var users = new SubSonic.Query.Select(Repository.GetProvider(), "ID", "NickName", "Avatar", "Signature").From<User>().Where("ID").In(articles.Select(x => x.CreateUserID).Distinct().ToArray()).ExecuteTypedList<User>();

                var array = list.Select(x => x.ArticleID).ToArray();
                var parts = new SubSonic.Query.Select(Repository.GetProvider()).From<ArticlePart>().Where<ArticlePart>(x => x.Types == 1).And("ArticleID").In(array).OrderAsc("ID").ExecuteTypedList<ArticlePart>();
                if (parts.Count > 3)
                {
                    parts = parts.Take(3).ToList();
                }

                var newlist = (from a in articles
                               join u in users on a.CreateUserID equals u.ID
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
                                   Pays = a.Pays,
                                   CreateDate = FormatTime(a.CreateDate),
                                   TypeName = articletypes.Exists(x => x.ID == a.TypeID) ? articletypes.FirstOrDefault(x => x.ID == a.TypeID).Name : "",
                                   ArticlePart = parts.Where(x => x.ArticleID == a.ID).OrderBy(x => x.ID).ToList()
                               }).ToList();
                var result = new
                {
                    currpage = pager.Index,
                    records = recordCount,
                    totalpage = totalPage,
                    list = newlist
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
