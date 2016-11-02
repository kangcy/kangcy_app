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
    /// 用户登录管理
    /// </summary>
    public class UserLoginController : BaseController
    {
        /// <summary>
        /// 删除
        /// </summary>
        public ActionResult Delete()
        {
            try
            {
                User user = GetUserInfo();
                if (user == null)
                {
                    return Json(new { result = false, message = "用户信息验证失败" }, JsonRequestBehavior.AllowGet);
                }
                var id = ZNRequest.GetInt("UserLoginID");
                var model = db.Single<UserLogin>(x => x.ID == id);
                if (model != null)
                {
                    model.Status = Enum_Status.Audit;
                    var result = db.Update<UserLogin>(model);
                    if (result > 0)
                    {
                        return Json(new { result = true, message = "成功" }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error(ex.Message);
            }
            return Json(new { result = false, message = "失败" }, JsonRequestBehavior.AllowGet);
        }
    }
}
