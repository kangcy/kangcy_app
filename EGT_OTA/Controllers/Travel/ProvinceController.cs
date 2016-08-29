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
    /// 省份
    /// </summary>
    public class ProvinceController : BaseController
    {
        public ActionResult Index()
        {
            ViewBag.CountryID = ZNRequest.GetInt("CountryID");
            return View();
        }

        public ActionResult Edit()
        {
            var id = ZNRequest.GetInt("id");
            var CountryID = ZNRequest.GetInt("CountryID");
            Province model = null;
            if (id > 0)
            {
                model = db.Single<Province>(x => x.ID == id);
            }
            if (model == null)
            {
                model = new Province();
            }
            model.CountryID = CountryID;
            return View(model);
        }

        /// <summary>
        /// 列表
        /// </summary>
        public ActionResult List()
        {
            var pager = new Pager();
            var countryID = ZNRequest.GetInt("CountryID");
            List<Province> all = db.Find<Province>(x => x.CountryID == countryID).ToList();

            string name = ZNRequest.GetString("Name");
            if (!string.IsNullOrEmpty(name))
            {
                all = all.FindAll(x => x.Name.Contains(name));
            }
            int recordCount = all.Count;
            var list = all;
            int totalPage = recordCount % pager.Size == 0 ? recordCount / pager.Size : recordCount / pager.Size + 1;

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
                Province model = new Province();
                model.ID = ZNRequest.GetInt("ID");
                if (model.ID > 0)
                {
                    model = db.Single<Province>(x => x.ID == model.ID);
                }
                model.Name = ZNRequest.GetString("Name");
                model.CountryID = ZNRequest.GetInt("CountryID");
                model.UpdateUserID = user.ID;
                model.UpdateDate = DateTime.Now;
                model.UpdateIP = Tools.GetClientIP;
                if (model.ID == 0)
                {
                    model.CreateUserID = user.ID;
                    model.CreateDate = DateTime.Now;
                    model.CreateIP = Tools.GetClientIP;
                    db.Add<Province>(model);
                }
                else
                {
                    db.Update<Province>(model);
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
            List<Province> list = new List<Province>();
            try
            {
                List<Province> all = db.All<Province>().ToList();
                var id = ids.Split(',');
                for (var i = 0; i < id.Length; i++)
                {
                    var index = Tools.SafeInt(id[i]);
                    var model = all.Single<Province>(x => x.ID == index);
                    if (model != null)
                    {
                        list.Add(model);
                    }
                }
                result = db.DeleteMany<Province>(list).ToString();
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
