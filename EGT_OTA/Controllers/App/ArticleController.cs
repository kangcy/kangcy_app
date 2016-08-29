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

                    User createuser = db.Single<User>(x => x.ID == model.CreateUserID);
                    model.UserName = createuser == null ? "" : createuser.NickName;
                    return Json(new { result = result, message = model }, JsonRequestBehavior.AllowGet);
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
            var query = new SubSonic.Query.Select(Repository.GetProvider(), "ID", "Title", "TypeID", "Cover", "Views", "Keeps", "Comments", "CreateUserID", "CreateDate").From<Article>().Where<Article>(x => x.Status == Enum_Status.Approved);

            //创建人
            var UserID = ZNRequest.GetInt("UserID");
            if (UserID > 0)
            {
                query = query.And("CreateUserID").IsEqualTo(UserID);
            }

            //文章类型
            var TypeID = ZNRequest.GetInt("TypeID");
            if (TypeID > 0)
            {
                query = query.And("TypeID").IsEqualTo(TypeID);
            }
            var recordCount = query.GetRecordCount();
            var totalPage = recordCount % pager.Size == 0 ? recordCount / pager.Size : recordCount / pager.Size + 1;
            var list = query.Paged(pager.Index, pager.Size).OrderDesc("ID").ExecuteTypedList<Article>();
            var types = new SubSonic.Query.Select(Repository.GetProvider(), "ID", "Name").From<ArticleType>().Where<ArticleType>(x => x.Status == Enum_Status.Approved).ExecuteTypedList<ArticleType>();
            var users = new SubSonic.Query.Select(Repository.GetProvider(), "ID", "NickName", "Avatar").From<User>().Where<User>(x => x.Status == Enum_Status.Approved).And("ID").In(list.Select(x => x.CreateUserID).ToArray()).ExecuteTypedList<User>();
            var newlist = (from a in list
                           join u in users on a.CreateUserID equals u.ID
                           join t in types on a.TypeID equals t.ID
                           select new
                           {
                               NickName = u.NickName,
                               Avatar = GetFullUrl(u.Avatar),
                               ArticleID = a.ID,
                               Title = a.Title,
                               Cover = a.Cover,
                               Views = a.Views,
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
