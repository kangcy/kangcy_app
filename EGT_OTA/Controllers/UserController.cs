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

namespace EGT_OTA.Controllers
{
    /// <summary>
    /// 用户管理
    /// </summary>
    public class UserController : BaseController
    {
        public ActionResult Index()
        {
            ViewBag.Role = RoleSelect(true, 0);
            return View();
        }

        public ActionResult Edit()
        {
            var id = ZNRequest.GetInt("id");
            UserInfo model = null;
            if (id > 0)
            {
                model = db.Single<UserInfo>(x => x.ID == id);
                model.Password = DesEncryptHelper.Decrypt(model.Password);
            }
            if (model == null)
            {
                model = new UserInfo();
            }
            ViewBag.Role = RoleSelect(false, model.RoleID);
            ViewBag.Sex = SexSelect(false, model.Sex);
            ViewBag.Status = UsedSelect(false, model.Status);
            return View(model);
        }

        /// <summary>
        /// 列表
        /// </summary>
        public ActionResult List()
        {
            var pager = new Pager();
            var RealName = ZNRequest.GetString("RealName");
            var UserName = ZNRequest.GetString("UserName");
            var RoleID = ZNRequest.GetInt("RoleID");

            var query = new SubSonic.Query.Select(Repository.GetProvider()).From<UserInfo>();
            if (RoleID > 0)
            {
                query = query.And("RoleID").IsEqualTo(RoleID);
            }
            if (!string.IsNullOrWhiteSpace(RealName))
            {
                query = query.And("RealName").Like("%" + RealName + "%");
            }
            if (!string.IsNullOrWhiteSpace(UserName))
            {
                query = query.And("UserName").Like("%" + UserName + "%");
            }
            var role = db.All<Role>().ToList();
            var recordCount = query.GetRecordCount();
            var totalPage = recordCount % pager.Size == 0 ? recordCount / pager.Size : recordCount / pager.Size + 1; //计算总页数
            var list = query.Paged(pager.Index, pager.Size).OrderDesc("ID").ExecuteTypedList<UserInfo>();
            var newlist = (from l in list
                           select new
                           {
                               ID = l.ID,
                               UserName = l.UserName,
                               RealName = l.RealName,
                               Sex = EnumBase.GetDescription(typeof(Enum_Sex), l.Sex),
                               RoleID = role.Exists(x => x.ID == l.RoleID) ? role.FirstOrDefault(x => x.ID == l.RoleID).Name : "未知",
                               Weixin = l.Weixin,
                               Email = l.Email,
                               QQ = l.QQ,
                               Age = l.Age,
                               CreateDate = l.CreateDate.ToString("yyyy-MM-dd hh:mm:ss"),
                               LastLoginDate = l.LastLoginDate.ToString("yyyy-MM-dd hh:mm:ss"),
                               LoginTimes = l.LoginTimes,
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
            if ((id == 0 && !CurrentUser.HasPower("21-2")) || (id > 0 && !CurrentUser.HasPower("21-3")))
            {
                return Json(new { result = result, message = "您不是管理员或者没有管理的权限" }, JsonRequestBehavior.AllowGet);
            }
            var UserName = ZNRequest.GetString("UserName");
            if (db.Exists<UserInfo>(x => x.UserName == UserName && x.ID != id))
            {
                return Json(new { result = "该用户名已被注册" }, JsonRequestBehavior.AllowGet);
            }
            UserInfo model = null;
            if (id > 0)
            {
                model = db.Single<UserInfo>(x => x.ID == id);
            }
            if (model == null)
            {
                model = new UserInfo();
            }
            model.Password = DesEncryptHelper.Encrypt(ZNRequest.GetString("Password")); ;
            model.UserName = UserName;
            model.Sex = ZNRequest.GetInt("Sex");
            model.Age = ZNRequest.GetInt("Age");
            model.Weixin = ZNRequest.GetString("Weixin");
            model.QQ = ZNRequest.GetString("QQ");
            model.Email = ZNRequest.GetString("Email");
            model.RealName = ZNRequest.GetString("RealName");
            model.RoleID = ZNRequest.GetInt("RoleID");
            model.Status = Enum_Status.Audit;
            try
            {
                if (model.ID == 0)
                {
                    model.CreateDate = DateTime.Now;
                    model.LastLoginDate = DateTime.Now;
                    model.LastLoginIP = Tools.GetClientIP;
                    model.LoginTimes = 0;
                    result = Tools.SafeInt(db.Add<UserInfo>(model)) > 0;
                }
                else
                {
                    result = db.Update<UserInfo>(model) > 0;
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
            if (!CurrentUser.HasPower("21-4"))
            {
                return Json(new { result = result, message = "您不是管理员或者没有管理的权限" }, JsonRequestBehavior.AllowGet);
            }
            var ids = ZNRequest.GetString("ids");
            List<UserInfo> list = new List<UserInfo>();
            try
            {
                List<UserInfo> all = db.All<UserInfo>().ToList();
                var id = ids.Split(',');
                for (var i = 0; i < id.Length; i++)
                {
                    var index = Tools.SafeInt(id[i]);
                    var model = all.Single<UserInfo>(x => x.ID == index);
                    if (model != null)
                    {
                        list.Add(model);
                    }
                }
                result = db.DeleteMany<UserInfo>(list) > 0;
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
            if ((status == Enum_Status.Approved && !CurrentUser.HasPower("21-7")) || (status == Enum_Status.Audit && !CurrentUser.HasPower("21-8")))
            {
                return Json(new { result = result, message = "您不是管理员或者没有管理的权限" }, JsonRequestBehavior.AllowGet);
            }
            var ids = ZNRequest.GetString("ids");
            List<UserInfo> list = new List<UserInfo>();
            try
            {
                List<UserInfo> all = db.All<UserInfo>().ToList();
                var id = ids.Split(',');
                for (var i = 0; i < id.Length; i++)
                {
                    var index = Tools.SafeInt(id[i]);
                    var model = all.Single<UserInfo>(x => x.ID == index);
                    if (model != null)
                    {
                        model.Status = status;
                    }
                    list.Add(model);
                }
                result = db.UpdateMany<UserInfo>(list) > 0;
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
