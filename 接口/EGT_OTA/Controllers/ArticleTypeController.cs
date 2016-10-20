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
    /// 文章类型管理
    /// </summary>
    public class ArticleTypeController : BaseController
    {
        /// <summary>
        /// 申请订阅
        /// </summary>
        public ActionResult Approve()
        {
            User user = GetUserInfo();
            if (user == null)
            {
                return Json(new { result = false, message = "用户信息验证失败" }, JsonRequestBehavior.AllowGet);
            }

            var result = false;
            var message = string.Empty;
            var id = ZNRequest.GetInt("ID");
            try
            {
                var model = db.Single<ArticleType>(x => x.ID == id);
                if (model == null)
                {
                    return Json(new { result = false, message = "类型不存在" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    model.Number += 1;
                    result = db.Update<ArticleType>(model) > 0;
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
        /// 列表
        /// </summary>
        public ActionResult All()
        {
            try
            {
                var list = new SubSonic.Query.Select(Repository.GetProvider()).From<ArticleType>().Where<ArticleType>(x => x.Status == Enum_Status.Approved).OrderAsc("SortID").ExecuteTypedList<ArticleType>();
                var newlist = (from l in list
                               select new
                               {
                                   ID = l.ID,
                                   Cover = GetFullUrl(l.Cover),
                                   Name = l.Name,
                                   Summary = l.Summary,
                                   CurrID = l.CurrID,
                                   ParentID = l.ParentID,
                                   ParentIDList = l.ParentIDList,
                                   Number = l.Number
                               }).ToList();
                var result = new
                {
                    currpage = 1,
                    records = list.Count(),
                    totalpage = 1,
                    list = newlist
                };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error(ex.Message);
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
