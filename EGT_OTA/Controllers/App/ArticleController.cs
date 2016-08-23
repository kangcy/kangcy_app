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
    public class ArticleController : BaseController
    {
        /// <summary>
        /// 文章列表
        /// </summary>
        public ActionResult ArticleList()
        {
            int currentPage = ZNRequest.GetInt("page", 0);///当前页
            int pageSize = ZNRequest.GetInt("ps", 15);//每页大小

            var query = new SubSonic.Query.Select(Repository.GetProvider(), "ID", "Title", "Cover", "Views", "Goods", "Comments").From<Article>().Where<Article>(x => x.Status != Enum_Status.DELETE);
            int recordCount = query.GetRecordCount();
            var list = query.Paged(currentPage, pageSize).OrderDesc("ID").ExecuteTypedList<Article>();
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 文章编辑
        /// </summary>
        public ActionResult ArticleManage()
        {
            var status = false;
            var result = string.Empty;
            try
            {
                Article model = new Article();
                model.ID = ZNRequest.GetInt("ID");
                model.Title = ZNRequest.GetString("Title");
                model.Introduction = SqlFilter(ZNRequest.GetString("Introduction"));
                model.Cover = ZNRequest.GetString("Cover");
                model.TypeID = ZNRequest.GetInt("TypeID", 0);
                model.Views = 0;
                model.Goods = 0;
                model.Comments = 0;
                model.Status = 0;
                var user = ZNRequest.GetInt("UserID");
                if (model.ID == 0)
                {
                    model.CreateUserID = user;
                    model.CreateDate = DateTime.Now;
                    model.CreateIP = Tools.GetClientIP;
                    status = Tools.SafeInt(db.Add<Article>(model)) > 0;
                }
                else
                {
                    model.UpdateUserID = user;
                    model.UpdateDate = DateTime.Now;
                    model.UpdateIP = Tools.GetClientIP;
                    status = db.Update<Article>(model) > 0;
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
        /// 文章点赞
        /// </summary>
        public ActionResult ArticleLike()
        {
            var status = false;
            var result = string.Empty;
            try
            {
                var id = ZNRequest.GetInt("id");
                if (id == 0)
                {
                    return Json(new { state = status, result = "参数异常" }, JsonRequestBehavior.AllowGet);
                }
                Article model = db.Single<Article>(x => x.ID == id);
                if (model == null)
                {
                    return Json(new { state = status, result = "数据异常" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    model.Views = model.Views + 1;
                    status = db.Update<Article>(model) > 0;
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
