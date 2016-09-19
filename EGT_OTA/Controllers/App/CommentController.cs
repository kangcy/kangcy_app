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
    /// <summary>
    /// 评论
    /// </summary>
    public class CommentController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 评论列表
        /// </summary>
        public ActionResult List()
        {
            var pager = new Pager();
            var query = new SubSonic.Query.Select(Repository.GetProvider()).From<Comment>();
            var recordCount = query.GetRecordCount();
            var totalPage = recordCount % pager.Size == 0 ? recordCount / pager.Size : recordCount / pager.Size + 1;
            var list = query.Paged(pager.Index, pager.Size).OrderDesc("ID").ExecuteTypedList<Comment>();
            var users = new SubSonic.Query.Select(Repository.GetProvider(), "ID", "NickName").From<User>().And("ID").In(list.Select(x => x.CreateUserID).ToArray()).ExecuteTypedList<User>();
            var articles = new SubSonic.Query.Select(Repository.GetProvider(), "ID", "Title").From<Article>().And("ID").In(list.Select(x => x.ArticleID).ToArray()).ExecuteTypedList<Article>();
            var newlist = (from l in list
                           select new
                           {
                               ID = l.ID,
                               ArticleID = l.ArticleID,
                               NickName = users.Exists(x => x.ID == l.CreateUserID) ? users.FirstOrDefault(x => x.ID == l.CreateUserID).NickName : "",
                               Title = articles.Exists(x => x.ID == l.ArticleID) ? articles.FirstOrDefault(x => x.ID == l.ArticleID).Title : "",
                               Summary = l.Summary,
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
        /// 删除
        /// </summary>
        public ActionResult Delete()
        {
            var result = false;
            var message = string.Empty;
            if (!CurrentUser.HasPower("35-4"))
            {
                return Json(new { result = result, message = "您不是管理员或者没有管理的权限" }, JsonRequestBehavior.AllowGet);
            }
            var ids = ZNRequest.GetString("ids");
            try
            {
                if (string.IsNullOrWhiteSpace(ids))
                {
                    message = "未选择任意项";
                }
                else
                {
                    var status = Enum_Status.DELETE;
                    var array = ids.Split(',').ToArray();
                    var list = new SubSonic.Query.Select(Repository.GetProvider()).From<Comment>().And("ID").In(array).ExecuteTypedList<Comment>();
                    list.ForEach(x =>
                    {
                        x.Status = status;
                    });
                    result = db.UpdateMany<Comment>(list) > 0;
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error(ex.Message, ex);
                message = ex.Message;
            }
            return Json(new { result = result, message = message }, JsonRequestBehavior.AllowGet);
        }

        #region  APP请求

        /// <summary>
        /// 评论编辑
        /// </summary>
        [AllowAnyone]
        public ActionResult Edit()
        {
            var callback = ZNRequest.GetString("jsoncallback");
            try
            {
                User user = GetUserInfo();
                if (user == null)
                {
                    return Content(callback + "(" + Newtonsoft.Json.JsonConvert.SerializeObject(new { result = false, message = "用户信息验证失败" }) + ")");
                }
                var articleID = ZNRequest.GetInt("ArticleID");
                if (articleID == 0)
                {
                    return Content(callback + "(" + Newtonsoft.Json.JsonConvert.SerializeObject(new { result = false, message = "文章信息异常" }) + ")");
                }
                var summary = SqlFilter(ZNRequest.GetString("Summary"));
                if (string.IsNullOrWhiteSpace(summary))
                {
                    return Content(callback + "(" + Newtonsoft.Json.JsonConvert.SerializeObject(new { result = false, message = "请填写评论内容" }) + ")");
                }
                Article article = new SubSonic.Query.Select(Repository.GetProvider(), "ID", "CreateUserID", "Comments").From<Article>().Where<Article>(x => x.ID == articleID).ExecuteSingle<Article>();
                if (article == null)
                {
                    return Content(callback + "(" + Newtonsoft.Json.JsonConvert.SerializeObject(new { result = false, message = "文章信息异常" }) + ")");
                }
                else
                {
                    Comment model = new Comment();
                    model.ArticleID = articleID;
                    model.ArticleUserID = article.CreateUserID;
                    model.Summary = summary;
                    model.Status = Enum_Status.Approved;
                    model.CreateDate = DateTime.Now;
                    model.CreateUserID = user.ID;
                    model.CreateIP = Tools.GetClientIP;
                    var result = Tools.SafeInt(db.Add<Comment>(model)) > 0;

                    //修改评论数
                    if (result)
                    {
                        var comments = article.Comments + 1;
                        result = new SubSonic.Query.Update<Article>(Repository.GetProvider()).Set("Comments").EqualTo(comments).Where<Article>(x => x.ID == articleID).Execute() > 0;
                        if (result)
                        {
                            return Content(callback + "(" + Newtonsoft.Json.JsonConvert.SerializeObject(new { result = true, message = comments }) + ")");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error(ex.Message);
            }
            return Content(callback + "(" + Newtonsoft.Json.JsonConvert.SerializeObject(new { result = false, message = "失败" }) + ")");
        }

        /// <summary>
        /// 列表
        /// </summary>
        [AllowAnyone]
        public ActionResult All()
        {
            var callback = ZNRequest.GetString("jsoncallback");
            try
            {
                var pager = new Pager();
                var query = new SubSonic.Query.Select(Repository.GetProvider()).From<Comment>().Where<Comment>(x => x.Status == Enum_Status.Approved);

                //评论人
                var CreateUserID = ZNRequest.GetInt("CreateUserID");
                if (CreateUserID > 0)
                {
                    query = query.And("CreateUserID").IsEqualTo(CreateUserID);
                }

                //文章作者
                var ArticleUserID = ZNRequest.GetInt("ArticleUserID");
                if (ArticleUserID > 0)
                {
                    query = query.And("ArticleUserID").IsEqualTo(ArticleUserID);
                }
                var recordCount = query.GetRecordCount();
                var totalPage = recordCount % pager.Size == 0 ? recordCount / pager.Size : recordCount / pager.Size + 1;
                var list = query.Paged(pager.Index, pager.Size).OrderDesc("ID").ExecuteTypedList<Comment>();
                var articles = new SubSonic.Query.Select(Repository.GetProvider(), "ID", "Title").From<Article>().Where("ID").In(list.Select(x => x.ArticleID).ToArray()).ExecuteTypedList<Article>();
                var users = new SubSonic.Query.Select(Repository.GetProvider(), "ID", "NickName", "Avatar").From<User>().Where("ID").In(list.Select(x => x.CreateUserID).ToArray()).ExecuteTypedList<User>();
                var newlist = (from l in list
                               join a in articles on l.ArticleID equals a.ID
                               join u in users on l.CreateUserID equals u.ID
                               select new
                               {
                                   ID = l.ID,
                                   Summary = l.Summary,
                                   CreateDate = l.CreateDate.ToString("yyyy-MM-dd"),
                                   NickName = u.NickName,
                                   Avatar = GetFullUrl(u.Avatar),
                                   ArticleID = a.ID,
                                   Title = a.Title
                               }).ToList();
                var result = new
                {
                    currpage = pager.Index,
                    records = recordCount,
                    totalpage = totalPage,
                    list = newlist
                };
                return Content(callback + "(" + Newtonsoft.Json.JsonConvert.SerializeObject(result) + ")");
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error(ex.Message);
                return Content(callback + "()");
            }
        }

        #endregion
    }
}
