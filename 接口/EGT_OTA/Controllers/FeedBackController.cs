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
    /// 意见反馈管理
    /// </summary>
    public class FeedBackController : BaseController
    {
        /// <summary>
        /// 编辑
        /// </summary>
        public ActionResult Edit()
        {
            var callback = ZNRequest.GetString("jsoncallback");
            try
            {
                User user = GetUserInfo();
                if (user == null)
                {
                    return Content(callback + "(" + Newtonsoft.Json.JsonConvert.SerializeObject(new { result = false, message = "用户信息验证失败" }) + ")");
                }
                var summary = SqlFilter(ZNRequest.GetString("Summary"));
                if (string.IsNullOrWhiteSpace(summary))
                {
                    return Content(callback + "(" + Newtonsoft.Json.JsonConvert.SerializeObject(new { result = false, message = "请填写反馈信息" }) + ")");
                }
                FeedBack model = new FeedBack();
                model.Summary = summary;
                model.CreateDate = DateTime.Now;
                model.CreateUserID = user.ID;
                model.CreateIP = Tools.GetClientIP;
                var result = Tools.SafeInt(db.Add<FeedBack>(model)) > 0;
                if (result)
                {
                    return Content(callback + "(" + Newtonsoft.Json.JsonConvert.SerializeObject(new { result = true, message = "成功" }) + ")");
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error(ex.Message);
            }
            return Content(callback + "(" + Newtonsoft.Json.JsonConvert.SerializeObject(new { result = false, message = "失败" }) + ")");
        }
    }
}
