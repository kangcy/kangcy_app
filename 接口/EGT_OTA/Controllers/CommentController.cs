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
        /// 评论点赞
        /// </summary>
        public ActionResult Zan()
        {
            try
            {
                User user = GetUserInfo();
                if (user == null)
                {
                    return Json(new { result = false, message = "用户信息验证失败" }, JsonRequestBehavior.AllowGet);
                }
                var commentID = ZNRequest.GetInt("CommentID");
                if (commentID == 0)
                {
                    return Json(new { result = false, message = "评论信息异常" }, JsonRequestBehavior.AllowGet);
                }
                Comment comment = db.Single<Comment>(x => x.ID == commentID);
                if (comment == null)
                {
                    return Json(new { result = false, message = "评论信息异常" }, JsonRequestBehavior.AllowGet);
                }

                Zan model = db.Single<Zan>(x => x.CreateUserID == user.ID && x.ArticleID == commentID && x.ZanType == Enum_Zan.Comment);
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
                model.ArticleID = commentID;
                model.ArticleUserID = comment.ArticleUserID;
                model.ZanType = Enum_Zan.Comment;
                var result = Tools.SafeInt(db.Add<Zan>(model)) > 0;
                if (result)
                {
                    comment.Goods += 1;
                    result = db.Update<Comment>(comment) > 0;
                }
                if (result)
                {
                    return Json(new { result = true, message = "成功" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error(ex.Message);
            }
            return Json(new { result = false, message = "失败" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 评论编辑
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
                var summary = SqlFilter(ZNRequest.GetString("Summary"));
                if (string.IsNullOrWhiteSpace(summary))
                {
                    return Json(new { result = false, message = "请填写评论内容" }, JsonRequestBehavior.AllowGet);
                }
                Article article = new SubSonic.Query.Select(Repository.GetProvider(), "ID", "CreateUserID", "Comments").From<Article>().Where<Article>(x => x.ID == articleID).ExecuteSingle<Article>();
                if (article == null)
                {
                    return Json(new { result = false, message = "文章信息异常" }, JsonRequestBehavior.AllowGet);
                }

                Comment model = new Comment();
                model.ArticleID = articleID;
                model.ArticleUserID = article.CreateUserID;
                model.Summary = summary;
                model.City = ZNRequest.GetString("City");
                model.Status = Enum_Status.Approved;
                model.CreateDate = DateTime.Now;
                model.CreateUserID = user.ID;
                model.CreateIP = Tools.GetClientIP;
                model.ParentCommentID = ZNRequest.GetInt("ParentCommentID");
                model.ParentUserID = ZNRequest.GetInt("ParentUserID");
                var result = Tools.SafeInt(db.Add<Comment>(model)) > 0;

                //修改评论数
                if (result)
                {
                    var comments = article.Comments + 1;
                    result = new SubSonic.Query.Update<Article>(Repository.GetProvider()).Set("Comments").EqualTo(comments).Where<Article>(x => x.ID == articleID).Execute() > 0;
                    if (result)
                    {
                        return Json(new { result = true, message = comments }, JsonRequestBehavior.AllowGet);
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
        /// 文章评论
        /// </summary>
        public ActionResult ArticleComment()
        {
            try
            {
                var pager = new Pager();
                var query = new SubSonic.Query.Select(Repository.GetProvider()).From<Comment>().Where<Comment>(x => x.Status == Enum_Status.Approved);

                //文章
                var ArticleID = ZNRequest.GetInt("ArticleID");
                if (ArticleID == 0)
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    query = query.And("ArticleID").IsEqualTo(ArticleID);
                }
                var recordCount = query.GetRecordCount();
                var totalPage = recordCount % pager.Size == 0 ? recordCount / pager.Size : recordCount / pager.Size + 1;
                var list = query.Paged(pager.Index, pager.Size).OrderAsc("ID").ExecuteTypedList<Comment>();

                //所有用户
                var users = new SubSonic.Query.Select(Repository.GetProvider(), "ID", "NickName", "Avatar").From<User>().Where("ID").In(list.Select(x => x.CreateUserID).Distinct().ToArray()).ExecuteTypedList<User>();

                //父评论
                var parentComment = new List<Comment>();
                var parentUser = new List<User>();
                if (list.Exists(x => x.ParentUserID > 0))
                {
                    parentComment = new SubSonic.Query.Select(Repository.GetProvider(), "ID", "Summary").From<Comment>().Where("ID").In(list.Select(x => x.ParentCommentID).Distinct().ToArray()).ExecuteTypedList<Comment>();
                    parentUser = new SubSonic.Query.Select(Repository.GetProvider(), "ID", "NickName", "Avatar").From<User>().Where("ID").In(list.Select(x => x.ParentUserID).Distinct().ToArray()).ExecuteTypedList<User>();
                }
                var newlist = (from l in list
                               join u in users on l.CreateUserID equals u.ID
                               select new
                               {
                                   ID = l.ID,
                                   Summary = l.Summary,
                                   City = l.City,
                                   Goods = l.Goods,
                                   CreateDate = FormatTime(l.CreateDate),
                                   UserID = u.ID,
                                   NickName = u.NickName,
                                   Avatar = GetFullUrl(u.Avatar),
                                   ParentCommentID = l.ParentCommentID,
                                   ParentUserID = l.ParentUserID,
                                   ParentNickName = l.ParentUserID == 0 ? "" : (parentUser.Exists(x => x.ID == l.ParentUserID) ? parentUser.FirstOrDefault(x => x.ID == l.ParentUserID).NickName : ""),
                                   ParentSummary = l.ParentCommentID == 0 ? "" : (parentComment.Exists(x => x.ID == l.ParentCommentID) ? parentComment.FirstOrDefault(x => x.ID == l.ParentCommentID).Summary : "")
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

        /// <summary>
        /// 列表
        /// </summary>
        public ActionResult All()
        {
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
                var list = query.Paged(pager.Index, pager.Size).OrderAsc("ID").ExecuteTypedList<Comment>();
                var articles = new SubSonic.Query.Select(Repository.GetProvider(), "ID", "Title", "ArticlePower").From<Article>().Where("ID").In(list.Select(x => x.ArticleID).ToArray()).ExecuteTypedList<Article>();
                var users = new SubSonic.Query.Select(Repository.GetProvider(), "ID", "NickName", "Avatar").From<User>().Where("ID").In(list.Select(x => x.CreateUserID).ToArray()).ExecuteTypedList<User>();
                var newlist = (from l in list
                               join a in articles on l.ArticleID equals a.ID
                               join u in users on l.CreateUserID equals u.ID
                               select new
                               {
                                   ID = l.ID,
                                   Summary = l.Summary,
                                   City = l.City,
                                   Goods = l.Goods,
                                   CreateDate = FormatTime(l.CreateDate),
                                   UserID = u.ID,
                                   NickName = u.NickName,
                                   Avatar = GetFullUrl(u.Avatar),
                                   ArticleID = a.ID,
                                   Title = a.Title,
                                   ArticlePower = a.ArticlePower
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
