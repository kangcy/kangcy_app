using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CommonTools;
using EGT_OTA.Helper;
using EGT_OTA.Models;

namespace EGT_OTA.Controllers
{
    /// <summary>
    /// 评论
    /// </summary>
    public class CommentController : BaseController
    {
        /// <summary>
        /// 评论编辑
        /// </summary>
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
                                   UserID = u.ID,
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
    }
}
