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
    /// 打赏管理
    /// </summary>
    public class PayController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Edit()
        {
            var id = ZNRequest.GetInt("id");
            Pay model = null;
            if (id > 0)
            {
                model = db.Single<Pay>(x => x.ID == id);
            }
            if (model == null)
            {
                model = new Pay();
            }
            return View(model);
        }

        /// <summary>
        /// 列表
        /// </summary>
        public ActionResult List()
        {
            var pager = new Pager();
            var query = new SubSonic.Query.Select(Repository.GetProvider()).From<Pay>();
            string Name = ZNRequest.GetString("Name");
            if (!string.IsNullOrWhiteSpace(Name))
            {
                var array = db.Find<User>(x => x.UserName.Contains(Name)).Select(x => x.ID).ToArray();
                if (array.Length > 0)
                {
                    query = query.And("CreateUserID").In(array);
                }
            }
            var recordCount = query.GetRecordCount();
            var totalPage = recordCount % pager.Size == 0 ? recordCount / pager.Size : recordCount / pager.Size + 1;
            var list = query.Paged(pager.Index, pager.Size).OrderDesc("ID").ExecuteTypedList<Pay>();

            var fromarray = list.Select(x => x.FromUserID).Distinct().ToList();
            var toarray = list.Select(x => x.ToUserID).Distinct().ToList();
            fromarray.AddRange(toarray);
            var allsers = new SubSonic.Query.Select(Repository.GetProvider(), "ID", "NickName").From<User>().And("ID").In(fromarray.ToArray()).ExecuteTypedList<User>();
            var newlist = (from l in list
                           select new
                           {
                               ID = l.ID,
                               FromUserName = allsers.Exists(x => x.ID == l.FromUserID) ? allsers.FirstOrDefault(x => x.ID == l.FromUserID).NickName : "",
                               ToUserName = allsers.Exists(x => x.ID == l.ToUserID) ? allsers.FirstOrDefault(x => x.ID == l.ToUserID).NickName : "",
                               CreateDate = l.CreateDate.ToString("yyyy-MM-dd hh:mm:ss"),
                               Money = l.Money,
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

        #region  APP请求

        /// <summary>
        /// 编辑
        /// </summary>
        [AllowAnyone]
        public ActionResult Manage()
        {
            User user = GetUserInfo();
            if (user == null)
            {
                return Json(new { result = false, message = "用户信息验证失败" }, JsonRequestBehavior.AllowGet);
            }

            var result = false;
            var message = string.Empty;
            Pay model = new Pay();
            model.FromUserID = user.ID;
            model.Status = Enum_Status.Approved;
            model.ToUserID = ZNRequest.GetInt("ToUserID");
            model.Money = user.ID;
            try
            {
                model.CreateDate = DateTime.Now;
                model.CreateIP = Tools.GetClientIP;
                result = Tools.SafeInt(db.Add<Pay>(model)) > 0;
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
        [AllowAnyone]
        public ActionResult Delete()
        {
            User user = GetUserInfo();
            if (user == null)
            {
                return Json(new { result = false, message = "用户信息验证失败" }, JsonRequestBehavior.AllowGet);
            }

            var result = false;
            var message = string.Empty;
            var id = ZNRequest.GetInt("ids");
            var model = db.Single<Pay>(x => x.ID == id);
            try
            {
                if (model != null)
                {
                    model.Status = Enum_Status.DELETE;
                    result = db.Update<Pay>(model) > 0;
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
        [AllowAnyone]
        public ActionResult All()
        {
            var pager = new Pager();
            var query = new SubSonic.Query.Select(Repository.GetProvider()).From<Pay>().Where<Pay>(x => x.Status == Enum_Status.Approved);
            var FromUserID = ZNRequest.GetInt("FromUserID");
            var ToUserID = ZNRequest.GetInt("ToUserID");
            if (FromUserID > 0)
            {
                query = query.And("FromUserID").IsEqualTo(FromUserID);
            }
            if (ToUserID > 0)
            {
                query = query.And("ToUserID").IsEqualTo(ToUserID);
            }
            var recordCount = query.GetRecordCount();
            var totalPage = recordCount % pager.Size == 0 ? recordCount / pager.Size : recordCount / pager.Size + 1;
            var list = query.Paged(pager.Index, pager.Size).OrderDesc("ID").ExecuteTypedList<Pay>();

            var fromarray = list.Select(x => x.FromUserID).Distinct().ToList();
            var toarray = list.Select(x => x.ToUserID).Distinct().ToList();
            fromarray.AddRange(toarray);
            var allusers = new SubSonic.Query.Select(Repository.GetProvider(), "ID", "NickName", "Avatar").From<User>().And("ID").In(fromarray.ToArray()).ExecuteTypedList<User>();
            var newlist = (from l in list
                           select new
                           {
                               ID = l.ID,
                               Avatar = GetFullUrl(allusers.Exists(x => x.ID == l.FromUserID) ? allusers.FirstOrDefault(x => x.ID == l.FromUserID).Avatar : ""),
                               FromUserName = allusers.Exists(x => x.ID == l.FromUserID) ? allusers.FirstOrDefault(x => x.ID == l.FromUserID).NickName : "",
                               ToUserName = allusers.Exists(x => x.ID == l.ToUserID) ? allusers.FirstOrDefault(x => x.ID == l.ToUserID).NickName : "",
                               CreateDate = l.CreateDate.ToString("yyyy-MM-dd"),
                               Money = l.Money
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

        #endregion
    }
}
