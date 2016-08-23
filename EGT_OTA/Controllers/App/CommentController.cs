using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CommonTools;
using EGT_OTA.Helper;
using EGT_OTA.Models;
using EGT_OTA.Models.App;

namespace EGT_OTA.Controllers.App
{
    public class CommentController : BaseController
    {
        /// <summary>
        /// 评论列表
        /// </summary>
        public ActionResult CommentList()
        {
            int currentPage = ZNRequest.GetInt("page", 0);///当前页
            int pageSize = ZNRequest.GetInt("ps", 15);//每页大小

            var query = new SubSonic.Query.Select(Repository.GetProvider()).From<Article>().Where<Article>(x => x.Status != Enum_Status.DELETE);
            int recordCount = query.GetRecordCount();
            var list = query.Paged(currentPage, pageSize).OrderDesc("ID").ExecuteTypedList<Article>();
            return Json(list, JsonRequestBehavior.AllowGet);
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
