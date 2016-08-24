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
    /// 文章
    /// </summary>
    public class ArticleController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 列表
        /// </summary>
        public ActionResult List()
        {
            var pager = new Pager();
            string Name = ZNRequest.GetString("Name");
            var query = new SubSonic.Query.Select(Repository.GetProvider(), "ID", "Title", "Cover", "Views", "Goods", "Comments", "CreateDate", "Status").From<Article>();
            if (!string.IsNullOrWhiteSpace(Name))
            {
                query = query.And("Title").Like("%" + Name + "%");
            }
            var recordCount = query.GetRecordCount();
            var totalPage = recordCount % pager.Size == 0 ? recordCount / pager.Size : recordCount / pager.Size + 1; //计算总页数
            var list = query.Paged(pager.Index, pager.Size).OrderDesc("ID").ExecuteTypedList<Article>();
            var newlist = (from l in list
                           select new
                           {
                               ID = l.ID,
                               Title = l.Title,
                               Cover = GetFullUrl(l.Cover),
                               Views = l.Views,
                               Goods = l.Goods,
                               Comments = l.Comments,
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

        #region  APP请求

        /// <summary>
        /// 编辑
        /// </summary>
        public ActionResult Edit()
        {
            User user = GetUserInfo();
            if (user == null)
            {
                return Json(new { result = false, message = "用户信息验证失败" }, JsonRequestBehavior.AllowGet);
            }

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
                if (model.ID == 0)
                {
                    model.CreateUserID = user.ID;
                    model.CreateDate = DateTime.Now;
                    model.CreateIP = Tools.GetClientIP;
                    status = Tools.SafeInt(db.Add<Article>(model)) > 0;
                }
                else
                {
                    model.UpdateUserID = user.ID;
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
        /// 详情
        /// </summary>
        [AllowAnyone]
        public ActionResult Detail()
        {
            User user = GetUserInfo();
            if (user == null)
            {
                return Json(new { result = false, message = "用户信息验证失败" }, JsonRequestBehavior.AllowGet);
            }

            var result = false;
            var message = string.Empty;
            try
            {
                var id = ZNRequest.GetInt("id");
                if (id == 0)
                {
                    return Json(new { result = result, message = "参数异常" }, JsonRequestBehavior.AllowGet);
                }
                Article model = db.Single<Article>(x => x.ID == id);
                if (model == null)
                {
                    return Json(new { result = result, message = "数据异常" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    model.Views += 1;
                    result = db.Update<Article>(model) > 0;
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error(ex.Message, ex);
                message = ex.Message;
            }
            return Json(new { status = result, message = message }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 点赞
        /// </summary>
        [AllowAnyone]
        public ActionResult Good()
        {
            User user = GetUserInfo();
            if (user == null)
            {
                return Json(new { result = false, message = "用户信息验证失败" }, JsonRequestBehavior.AllowGet);
            }

            var result = false;
            var message = string.Empty;
            try
            {
                var id = ZNRequest.GetInt("id");
                if (id == 0)
                {
                    return Json(new { result = result, message = "参数异常" }, JsonRequestBehavior.AllowGet);
                }
                Article model = db.Single<Article>(x => x.ID == id);
                if (model == null)
                {
                    return Json(new { result = result, message = "数据异常" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    model.Goods = model.Goods + 1;
                    result = db.Update<Article>(model) > 0;

                    if (result)
                    {
                        Handle handle = new Handle();
                        handle.ArticleID = model.ID;
                        handle.UserID = user.ID;
                        handle.CreateDate = DateTime.Now;
                        db.Add<Handle>(handle);
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

        #endregion
    }
}
