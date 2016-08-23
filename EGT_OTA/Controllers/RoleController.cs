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
            ViewBag.Used = UsedSelect(false, model.Status);
            if (string.IsNullOrEmpty(model.Auth))
            {
                model.Auth = string.Empty;
            }
            ViewBag.MenuTree = MenuTree(model.Auth);
            return View(model);
        }


        /// <summary>
        /// 权限管理
        /// </summary>
        /// <returns></returns>
        public ActionResult Jurisdiction()
        {
            return View();
        }

        /// <summary>
        /// 菜单管理
        /// </summary>
        /// <returns></returns>
        public ActionResult Menus()
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
            var all = db.All<Role>().ToList();
            if (!string.IsNullOrEmpty(Name))
            {
                all = all.FindAll(x => x.Name.Contains(Name));
            }
            int recordCount = all.Count;
            var list = all;
            int totalPage = recordCount % pager.Size == 0 ? recordCount / pager.Size : recordCount / pager.Size + 1; // 计算总页数 
            StringWriter sw = new StringWriter();
            JsonWriter writer = new JsonTextWriter(sw);
            writer.WriteStartObject();
            writer.WritePropertyName("page");
            writer.WriteValue(pager.Index);
            writer.WritePropertyName("records");
            writer.WriteValue(recordCount);
            writer.WritePropertyName("total");
            writer.WriteValue(totalPage);
            writer.WritePropertyName("rows");
            writer.WriteStartArray();
            for (int i = 0, len = list.Count; i < len; i++)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("ID");
                writer.WriteValue(list[i].ID);
                writer.WritePropertyName("Name");
                writer.WriteValue(list[i].Name);
                writer.WritePropertyName("CreateDate");
                writer.WriteValue(list[i].CreateDate.ToString("yyyy-MM-dd hh:mm:ss"));
                writer.WritePropertyName("Status");
                writer.WriteValue(list[i].Status);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
            writer.WriteEndObject();
            writer.Flush();
            writer.Close();
            return Content(sw.GetStringBuilder().ToString());
        }

        /// <summary>
        /// 编辑
        /// </summary>
        public ActionResult Manage()
        {
            int id = ZNRequest.GetInt("ID");

            //新增、编辑权限
            if ((id == 0 && !CurrentUser.HasPower("22-2")) || (id > 0 && !CurrentUser.HasPower("22-3")))
            {
                return Json(new { result = "您不是管理员或者没有管理的权限" }, JsonRequestBehavior.AllowGet);
            }

            var UserName = ZNRequest.GetString("UserName");
            if (db.Exists<UserInfo>(x => x.UserName == UserName))
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
            model.Name = ZNRequest.GetString("Name");
            model.Auth = ZNRequest.GetString("Auth");
            model.Status = ZNRequest.GetInt("Status");
            model.UpdateDate = DateTime.Now;
            model.UpdateUserID = user.ID;
            model.UpdateIP = Tools.GetClientIP;
            var result = false;
            var message = string.Empty;
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
            if (!CurrentUser.HasPower(""))
            {
                return Json(new { result = "您不是管理员或者没有管理的权限" }, JsonRequestBehavior.AllowGet);
            }
            string result = "0";
            var id = ZNRequest.GetInt("ids");
            var model = db.Single<Role>(x => x.ID == id);
            try
            {
                if (model != null)
                {
                    //判断是否存在用户
                    if (!db.Exists<UserInfo>(x => x.RoleID == id))
                    {
                        result = db.Delete<Role>(id).ToString();
                    }
                    else
                    {
                        result = "存在关联用户";
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error(ex.Message, ex);
                result = ex.Message;
            }
            return Json(new { result = result }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 审核
        /// </summary>
        public ActionResult Audit()
        {
            if (!CurrentUser.HasPower(""))
            {
                return Json(new { result = "您不是管理员或者没有管理的权限" }, JsonRequestBehavior.AllowGet);
            }
            string result = "0";
            int status = ZNRequest.GetInt("status");
            status = status == 1 ? Enum_Status.Approved : Enum_Status.Audit;
            var ids = ZNRequest.GetString("ids");
            List<Role> list = new List<Role>();
            try
            {
                List<Role> all = db.All<Role>().ToList();
                var id = ids.Split(',');
                for (var i = 0; i < id.Length; i++)
                {
                    var index = Tools.SafeInt(id[i]);
                    var model = all.Single<Role>(x => x.ID == index);
                    if (model != null)
                    {
                        model.Status = status;
                    }
                    list.Add(model);
                }
                result = db.UpdateMany<Role>(list).ToString();
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error(ex.Message, ex);
                result = ex.Message;
            }
            return Json(new { result = result }, JsonRequestBehavior.AllowGet);
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
