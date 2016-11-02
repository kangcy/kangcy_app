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
    /// 系统管理
    /// </summary>
    public class SystemController : BaseController
    {
        /// <summary>
        /// 检查更新
        /// </summary>
        public ActionResult CheckUpdate()
        {
            try
            {
                var client = ZNRequest.GetString("client");
                var version = ZNRequest.GetString("version");
                var currVersion = "";
                var currUrl = "";
                if (client == "android")
                {
                    currVersion = System.Configuration.ConfigurationManager.AppSettings["curr_android_version"];
                    currUrl = System.Configuration.ConfigurationManager.AppSettings["curr_android_url"];
                }
                else
                {
                    currVersion = System.Configuration.ConfigurationManager.AppSettings["curr_ios_version"];
                    currUrl = System.Configuration.ConfigurationManager.AppSettings["curr_ios_url"];
                }

                if (version == currVersion)
                {
                    return Json(new { result = false, message = "当前已是最新版本" }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { result = true, version = currVersion, url = currUrl }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error(ex.Message);
            }
            return Json(new { result = false, message = "失败" }, JsonRequestBehavior.AllowGet);
        }
    }
}
