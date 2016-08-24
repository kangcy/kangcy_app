using System;
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
    /// 收藏管理
    /// </summary>
    public class KeepController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Edit()
        {
            var id = ZNRequest.GetInt("id");
            Keep model = null;
            if (id > 0)
            {
                model = db.Single<Keep>(x => x.ID == id);
            }
            if (model == null)
            {
                model = new Keep();
            }
            return View(model);
        }

        /// <summary>
        /// 列表
        /// </summary>
        public ActionResult List()
        {
            var pager = new Pager();
            var query = new SubSonic.Query.Select(Repository.GetProvider()).From<Keep>();
            string Name = ZNRequest.GetString("Name");
            if (!string.IsNullOrWhiteSpace(Name))
            {
                var array = db.Find<User>(x => x.UserName == Name).Select(x => x.ID).ToArray();
                if (array.Length > 0)
                {
                    query = query.And("CreateUserID").In(array);
                }
            }
            var recordCount = query.GetRecordCount();
            var totalPage = recordCount % pager.Size == 0 ? recordCount / pager.Size : recordCount / pager.Size + 1; //计算总页数
            var list = query.Paged(pager.Index, pager.Size).OrderDesc("ID").ExecuteTypedList<Keep>();
            var users = new SubSonic.Query.Select(Repository.GetProvider(), "ID", "UserName").From<User>().And("ID").In(list.Select(x => x.CreateUserID).ToArray()).ExecuteTypedList<User>();
            var articles = new SubSonic.Query.Select(Repository.GetProvider(), "ID", "Title").From<User>().And("ID").In(list.Select(x => x.ArticleID).ToArray()).ExecuteTypedList<Article>();
            var newlist = (from l in list
                           select new
                           {
                               ID = l.ID,
                               UserName = users.Exists(x => x.ID == l.CreateUserID) ? users.FirstOrDefault(x => x.ID == l.CreateUserID).UserName : "",
                               ArticleName = articles.Exists(x => x.ID == l.ArticleID) ? articles.FirstOrDefault(x => x.ID == l.ArticleID).Title : "",
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
        /// 编辑
        /// </summary>
        [AllowAnyone]
        public ActionResult Manage()
        {
            var username = ZNRequest.GetString("Name");
            var password = DesEncryptHelper.Encrypt(ZNRequest.GetString("Password"));
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return Json(new { result = false, message = "用户信息验证失败" }, JsonRequestBehavior.AllowGet);
            }
            var user = db.Single<User>(x => x.UserName == username && x.Password == password);
            if (user == null)
            {
                return Json(new { result = false, message = "用户信息验证失败" }, JsonRequestBehavior.AllowGet);
            }

            var result = false;
            var message = string.Empty;
            var articleID = ZNRequest.GetInt("ArticleID");
            Keep model = db.Single<Keep>(x => x.CreateUserID == user.ID && x.ArticleID == articleID);
            if (model == null)
            {
                model = new Keep();
            }
            model.ArticleID = articleID;
            model.Status = Enum_Status.Approved;
            model.UpdateDate = DateTime.Now;
            model.UpdateUserID = user.ID;
            model.UpdateIP = Tools.GetClientIP;
            try
            {
                if (model.ID == 0)
                {
                    model.CreateDate = DateTime.Now;
                    model.CreateUserID = user.ID;
                    model.CreateIP = Tools.GetClientIP;
                    result = Tools.SafeInt(db.Add<Keep>(model)) > 0;
                }
                else
                {
                    result = db.Update<Keep>(model) > 0;
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
        public ActionResult Delete()
        {
            var result = false;
            var message = string.Empty;
            if (!CurrentUser.HasPower("34-4"))
            {
                return Json(new { result = result, message = "您不是管理员或者没有管理的权限" }, JsonRequestBehavior.AllowGet);
            }
            var id = ZNRequest.GetInt("ids");
            var model = db.Single<Keep>(x => x.ID == id);
            try
            {
                if (model != null)
                {
                    model.Status = Enum_Status.DELETE;
                    result = db.Update<Keep>(model) > 0;
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
        /// 审核
        /// </summary>
        public ActionResult Audit()
        {
            var result = false;
            var message = string.Empty;
            int status = ZNRequest.GetInt("status");
            if ((status == Enum_Status.Approved && !CurrentUser.HasPower("11-5")) || (status == Enum_Status.Audit && !CurrentUser.HasPower("11-6")))
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
                    var array = ids.Split(',').ToArray();
                    var list = new SubSonic.Query.Select(Repository.GetProvider()).From<Keep>().And("ID").In(array).ExecuteTypedList<Keep>();
                    list.ForEach(x =>
                    {
                        x.Status = status;
                    });
                    result = db.UpdateMany<Keep>(list) > 0;
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error(ex.Message, ex);
                message = ex.Message;
            }
            return Json(new { result = result, message = message }, JsonRequestBehavior.AllowGet);
        }
    }
}
