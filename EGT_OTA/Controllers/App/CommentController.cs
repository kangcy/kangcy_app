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
            var users = new SubSonic.Query.Select(Repository.GetProvider(), "ID", "UserName").From<User>().And("ID").In(list.Select(x => x.CreateUserID).ToArray()).ExecuteTypedList<User>();
            var articles = new SubSonic.Query.Select(Repository.GetProvider(), "ID", "Title").From<Article>().And("ID").In(list.Select(x => x.ArticleID).ToArray()).ExecuteTypedList<Article>();
            var newlist = (from l in list
                           select new
                           {
                               ID = l.ID,
                               ArticleID = l.ArticleID,
                               UserName = users.Exists(x => x.ID == l.CreateUserID) ? users.FirstOrDefault(x => x.ID == l.CreateUserID).UserName : "",
                               ArticleName = articles.Exists(x => x.ID == l.ArticleID) ? articles.FirstOrDefault(x => x.ID == l.ArticleID).Title : "",
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

        /// <summary>
        /// 评论编辑
        /// </summary>
        public ActionResult CommentManage()
        {
            var status = false;
            var result = string.Empty;
            try
            {
                Comment model = new Comment();
                model.ID = ZNRequest.GetInt("ID");
                model.Summary = ZNRequest.GetString("Summary");
                model.ArticleID = ZNRequest.GetInt("ArticleID", 0);
                var user = ZNRequest.GetInt("UserID");
                if (model.ID == 0)
                {
                    model.CreateUserID = user;
                    model.CreateDate = DateTime.Now;
                    model.CreateIP = Tools.GetClientIP;
                    status = Tools.SafeInt(db.Add<Comment>(model)) > 0;
                }
                else
                {
                    model.UpdateUserID = user;
                    model.UpdateDate = DateTime.Now;
                    model.UpdateIP = Tools.GetClientIP;
                    status = db.Update<Comment>(model) > 0;
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
