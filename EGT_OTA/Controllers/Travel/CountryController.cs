using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SubSonic.Repository;
using EGT_OTA.Models;
using CommonTools;
using System.IO;
using Newtonsoft.Json;
using EGT_OTA.Helper;

namespace EGT_OTA.Controllers
{
    /// <summary>
    /// 国家
    /// </summary>
    public class CountryController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Edit()
        {
            var id = ZNRequest.GetInt("id");
            Country model = null;
            if (id > 0)
            {
                model = db.Single<Country>(x => x.ID == id);
            }
            if (model == null)
            {
                model = new Country();
            }
            return View(model);
        }

        /// <summary>
        /// 列表
        /// </summary>
        public ActionResult List()
        {
            var pager = new Pager();
            List<Country> all = db.All<Country>().ToList();

            string name = ZNRequest.GetString("Name");
            if (!string.IsNullOrEmpty(name))
            {
                all = all.FindAll(x => x.Name.Contains(name));
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
                writer.WriteValue(Tools.FormatDateTime(list[i].CreateDate));
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
            if (!CurrentUser.HasPower(""))
            {
                return Json(new { result = "您不是管理员或者没有管理的权限" }, JsonRequestBehavior.AllowGet);
            }
            var result = "0";
            try
            {
                UserInfo user = CurrentUser.User;
                Country model = new Country();
                model.ID = ZNRequest.GetInt("ID");
                if (model.ID > 0)
                {
                    model = db.Single<Country>(x => x.ID == model.ID);
                }
                model.Name = ZNRequest.GetString("Name");
                model.UpdateUserID = user.ID;
                model.UpdateDate = DateTime.Now;
                model.UpdateIP = Tools.GetClientIP;
                if (model.ID == 0)
                {
                    model.CreateUserID = user.ID;
                    model.CreateDate = DateTime.Now;
                    model.CreateIP = Tools.GetClientIP;
                    db.Add<Country>(model);
                }
                else
                {
                    db.Update<Country>(model);
                }
                result = "1";
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error(ex.Message, ex);
                result = ex.Message;
            }
            return Json(new { result = result }, JsonRequestBehavior.AllowGet);
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
            var ids = ZNRequest.GetString("ids");
            List<Country> list = new List<Country>();
            try
            {
                List<Country> all = db.All<Country>().ToList();
                var id = ids.Split(',');
                for (var i = 0; i < id.Length; i++)
                {
                    var index = Tools.SafeInt(id[i]);
                    var model = all.Single<Country>(x => x.ID == index);
                    if (model != null)
                    {
                        list.Add(model);
                    }
                }
                result = db.DeleteMany<Country>(list).ToString();
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error(ex.Message, ex);
                result = ex.Message;
            }
            return Json(new { result = result }, JsonRequestBehavior.AllowGet);
        }
    }
}
