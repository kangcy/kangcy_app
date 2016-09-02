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
    /// 帮助中心
    /// </summary>
    public class HelpController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Edit()
        {
            var id = ZNRequest.GetInt("id");
            Help model = null;
            if (id > 0)
            {
                model = db.Single<Help>(x => x.ID == id);
            }
            if (model == null)
            {
                model = new Help();
            }
            return View(model);
        }

        /// <summary>
        /// 列表
        /// </summary>
        public ActionResult List()
        {
            var pager = new Pager();
            string Name = ZNRequest.GetString("Name");
            var query = new SubSonic.Query.Select(Repository.GetProvider(), "ID", "Name", "Summary", "CreateDate", "Status").From<Help>();
            if (!string.IsNullOrWhiteSpace(Name))
            {
                query = query.And("Name").Like("%" + Name + "%");
            }
            var recordCount = query.GetRecordCount();
            var totalPage = recordCount % pager.Size == 0 ? recordCount / pager.Size : recordCount / pager.Size + 1;
            var list = query.Paged(pager.Index, pager.Size).OrderDesc("ID").ExecuteTypedList<Help>();
            var newlist = (from l in list
                           select new
                           {
                               ID = l.ID,
                               Name = l.Name,
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
        /// 编辑
        /// </summary>
        public ActionResult Manage()
        {
            var result = false;
            var message = string.Empty;
            int id = ZNRequest.GetInt("ID");
            if ((id == 0 && !CurrentUser.HasPower("39-2")) || (id > 0 && !CurrentUser.HasPower("39-3")))
            {
                return Json(new { result = result, message = "您不是管理员或者没有管理的权限" }, JsonRequestBehavior.AllowGet);
            }
            var Name = ZNRequest.GetString("Name");
            if (string.IsNullOrWhiteSpace(Name))
            {
                return Json(new { result = "请填写文件名称" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                if (db.Exists<Help>(x => x.Name == Name))
                {
                    return Json(new { result = "该名称已存在" }, JsonRequestBehavior.AllowGet);
                }
            }
            UserInfo user = CurrentUser.User;
            Help model = null;

            if (id > 0)
            {
                model = db.Single<Help>(x => x.ID == id);
            }
            if (model == null)
            {
                model = new Help();
            }
            model.Summary = SqlFilter(ZNRequest.GetString("Summary"));
            model.Introduction = SqlFilter(ZNRequest.GetString("Introduction"));
            model.Name = Name;
            model.Status = Enum_Status.Audit;
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
                    result = Tools.SafeInt(db.Add<Help>(model)) > 0;
                }
                else
                {
                    result = db.Update<Help>(model) > 0;
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
            if (!CurrentUser.HasPower("39-4"))
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
                    var list = new SubSonic.Query.Select(Repository.GetProvider()).From<Video>().And("ID").In(array).ExecuteTypedList<Video>();
                    list.ForEach(x =>
                    {
                        x.Status = status;
                    });
                    result = db.UpdateMany<Video>(list) > 0;
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
            if ((status == Enum_Status.Approved && !CurrentUser.HasPower("39-5")) || (status == Enum_Status.Audit && !CurrentUser.HasPower("39-6")))
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
                    var list = new SubSonic.Query.Select(Repository.GetProvider()).From<Help>().And("ID").In(array).ExecuteTypedList<Help>();
                    list.ForEach(x =>
                    {
                        x.Status = status;
                    });
                    result = db.UpdateMany<Help>(list) > 0;
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
        /// 列表
        /// </summary>
        [AllowAnyone]
        public ActionResult All()
        {
            var callback = ZNRequest.GetString("jsoncallback");
            try
            {
                var pager = new Pager();
                var query = new SubSonic.Query.Select(Repository.GetProvider(), "ID", "Name", "Summary").From<Help>().Where<Help>(x => x.Status == Enum_Status.Approved);
                var recordCount = query.GetRecordCount();
                var totalPage = recordCount % pager.Size == 0 ? recordCount / pager.Size : recordCount / pager.Size + 1;
                var list = query.Paged(pager.Index, pager.Size).OrderDesc("ID").ExecuteTypedList<Help>();
                var newlist = (from l in list
                               select new
                               {
                                   ID = l.ID,
                                   Name = l.Name,
                                   Summary = l.Summary
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
