using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CommonTools;
using EGT_OTA.Helper;
using EGT_OTA.Models;

namespace EGT_OTA.Controllers.App
{
    public class ArticleTypeController : BaseController
    {
        /// <summary>
        /// 文章类型列表
        /// </summary>
        public ActionResult ArticleTypeList()
        {
            var list = new SubSonic.Query.Select(Repository.GetProvider()).From<ArticleType>().OrderDesc("ID").ExecuteTypedList<ArticleType>();
            int recordCount = list.Count;
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 文章类型编辑
        /// </summary>
        public ActionResult ArticleTypeManage()
        {
            var status = false;
            var result = string.Empty;
            try
            {
                ArticleType model = new ArticleType();
                model.ID = ZNRequest.GetInt("ID");
                model.Name = ZNRequest.GetString("Summary");
                model.Cover = ZNRequest.GetString("Cover");
                var user = ZNRequest.GetInt("UserID");
                if (model.ID == 0)
                {
                    model.CreateUserID = user;
                    model.CreateDate = DateTime.Now;
                    model.CreateIP = Tools.GetClientIP;
                    status = Tools.SafeInt(db.Add<ArticleType>(model)) > 0;
                }
                else
                {
                    model.UpdateUserID = user;
                    model.UpdateDate = DateTime.Now;
                    model.UpdateIP = Tools.GetClientIP;
                    status = db.Update<ArticleType>(model) > 0;
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error(ex.Message, ex);
                result = ex.Message;
            }
            return Json(new { status = status, result = result }, JsonRequestBehavior.AllowGet);
        }
    }
}
