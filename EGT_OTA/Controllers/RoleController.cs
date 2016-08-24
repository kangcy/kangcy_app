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
    /// 角色管理
    /// </summary>
    public class RoleController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Edit()
        {
            var id = ZNRequest.GetInt("id");
            Role model = null;
            if (id > 0)
            {
                model = db.Single<Role>(x => x.ID == id);
            }
            if (model == null)
            {
                model = new Role();
                model.Auth = "0";
            }
            if (string.IsNullOrEmpty(model.Auth))
            {
                model.Auth = string.Empty;
            }
            ViewBag.MenuTree = MenuTree(model.Auth);
            return View(model);
        }

        /// <summary>
        /// 列表
        /// </summary>
        public ActionResult List()
        {
            var pager = new Pager();
            string Name = ZNRequest.GetString("Name");
            var query = new SubSonic.Query.Select(Repository.GetProvider()).From<Role>();
            if (!string.IsNullOrWhiteSpace(Name))
            {
                query = query.And("Name").Like("%" + Name + "%");
            }
            var recordCount = query.GetRecordCount();
            var totalPage = recordCount % pager.Size == 0 ? recordCount / pager.Size : recordCount / pager.Size + 1; //计算总页数
            var list = query.Paged(pager.Index, pager.Size).OrderDesc("ID").ExecuteTypedList<Role>();
            var newlist = (from l in list
                           select new
                           {
                               ID = l.ID,
                               Name = l.Name,
                               CreateDate = l.CreateDate.ToString("yyyy-MM-dd hh:mm:ss"),
                               Status = l.Status
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
            if ((id == 0 && !CurrentUser.HasPower("22-2")) || (id > 0 && !CurrentUser.HasPower("22-3")))
            {
                return Json(new { result = result, message = "您不是管理员或者没有管理的权限" }, JsonRequestBehavior.AllowGet);
            }
            var Name = ZNRequest.GetString("Name");
            if (db.Exists<Role>(x => x.Name == Name))
            {
                return Json(new { result = "该角色名已被注册" }, JsonRequestBehavior.AllowGet);
            }
            UserInfo user = CurrentUser.User;
            Role model = null;

            if (id > 0)
            {
                model = db.Single<Role>(x => x.ID == id);
            }
            if (model == null)
            {
                model = new Role();
            }
            model.Name = Name;
            model.Auth = ZNRequest.GetString("Auth");
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
                    result = Tools.SafeInt(db.Add<Role>(model)) > 0;
                }
                else
                {
                    result = db.Update<Role>(model) > 0;
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
            if (!CurrentUser.HasPower("22-4"))
            {
                return Json(new { result = result, message = "您不是管理员或者没有管理的权限" }, JsonRequestBehavior.AllowGet);
            }
            var id = ZNRequest.GetInt("ids");
            var model = db.Single<Role>(x => x.ID == id);
            try
            {
                if (model != null)
                {
                    if (!db.Exists<UserInfo>(x => x.RoleID == id))
                    {
                        result = db.Delete<Role>(id) > 0;
                    }
                    else
                    {
                        message = "存在关联用户";
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

        /// <summary>
        /// 审核
        /// </summary>
        public ActionResult Audit()
        {
            var result = false;
            var message = string.Empty;
            int status = ZNRequest.GetInt("status");
            if ((status == Enum_Status.Approved && !CurrentUser.HasPower("22-7")) || (status == Enum_Status.Audit && !CurrentUser.HasPower("22-8")))
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
                    var list = new SubSonic.Query.Select(Repository.GetProvider()).From<Role>().And("ID").In(array).ExecuteTypedList<Role>();
                    list.ForEach(x =>
                    {
                        x.Status = status;
                    });
                    result = db.UpdateMany<Role>(list) > 0;
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
        /// 菜单树
        /// </summary>
        protected string MenuTree(string auth)
        {
            string menuStr = MenuStr();
            string buttonStr = ButtonStr();
            List<Button> buttonList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Button>>(buttonStr);
            List<AdminMenu> list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<AdminMenu>>(menuStr);
            List<AdminMenu> firstList = list.FindAll(x => x.ParentId == "0");

            JArray array = new JArray();
            firstList.ForEach(x =>
            {
                BuildFileChildNodes(x, array, list, buttonList, auth);
            });
            return Newtonsoft.Json.JsonConvert.SerializeObject(array);
        }

        /// <summary>
        /// 递归属性类型的json对象
        /// </summary>
        protected void BuildFileChildNodes(AdminMenu o, JArray array, List<AdminMenu> list, List<Button> buttonList, string auth)
        {
            array.Add(new JObject(
                new Newtonsoft.Json.Linq.JProperty("id", o.ModuleId),
                new Newtonsoft.Json.Linq.JProperty("name", o.FullName),
                new Newtonsoft.Json.Linq.JProperty("pId", o.ParentId),
                new Newtonsoft.Json.Linq.JProperty("auth", o.ModuleId),
                new Newtonsoft.Json.Linq.JProperty("checked", auth.Contains("," + o.ModuleId + ",") ? true : false),
                new Newtonsoft.Json.Linq.JProperty("open", true))
            );

            if (!string.IsNullOrEmpty(o.Button))
            {
                buttonList.ForEach(x =>
                {
                    if (o.Button.Contains(x.FullName))
                    {
                        array.Add(new JObject(
                           new Newtonsoft.Json.Linq.JProperty("id", o.ModuleId + "_" + x.ButtonId),
                           new Newtonsoft.Json.Linq.JProperty("name", x.FullName),
                           new Newtonsoft.Json.Linq.JProperty("pId", o.ModuleId),
                           new Newtonsoft.Json.Linq.JProperty("auth", o.ModuleId + "_" + x.ButtonId),
                           new Newtonsoft.Json.Linq.JProperty("checked", auth.Contains("," + o.ModuleId + "_" + x.ButtonId + ",") ? true : false),
                           new Newtonsoft.Json.Linq.JProperty("open", true))
                       );
                    }
                });
            }

            List<AdminMenu> newlist = list.FindAll(x => x.ParentId == o.ModuleId);
            if (newlist.Count > 0)
            {
                foreach (AdminMenu menu in newlist)
                {
                    BuildFileChildNodes(menu, array, list, buttonList, auth);
                }
            }
        }
    }
}
