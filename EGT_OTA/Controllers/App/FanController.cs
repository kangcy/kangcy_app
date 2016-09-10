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
    /// 关注、粉丝管理
    /// </summary>
    public class FanController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 列表
        /// </summary>
        public ActionResult List()
        {
            var pager = new Pager();
            var query = new SubSonic.Query.Select(Repository.GetProvider()).From<Fan>();
            string Name = ZNRequest.GetString("Name");
            if (!string.IsNullOrWhiteSpace(Name))
            {
                var array = db.Find<User>(x => x.NickName.Contains(Name)).Select(x => x.ID).ToArray();
                if (array.Length > 0)
                {
                    query = query.And("CreateUserID").In(array);
                }
            }
            string FanName = ZNRequest.GetString("FanName");
            if (!string.IsNullOrWhiteSpace(FanName))
            {
                var array = db.Find<User>(x => x.UserName == FanName).Select(x => x.ID).ToArray();
                if (array.Length > 0)
                {
                    query = query.And("UserID").In(array);
                }
            }
            var recordCount = query.GetRecordCount();
            var totalPage = recordCount % pager.Size == 0 ? recordCount / pager.Size : recordCount / pager.Size + 1;
            var list = query.Paged(pager.Index, pager.Size).OrderDesc("ID").ExecuteTypedList<Fan>();
            var userArray = list.Select(x => x.FromUserID).ToList();
            userArray.AddRange(list.Select(x => x.ToUserID).ToList());
            var users = new SubSonic.Query.Select(Repository.GetProvider(), "ID", "NickName").From<User>().And("ID").In(userArray.ToArray()).ExecuteTypedList<User>();
            var newlist = (from l in list
                           select new
                           {
                               ID = l.ID,
                               Name = users.Exists(x => x.ID == l.FromUserID) ? users.FirstOrDefault(x => x.ID == l.FromUserID).NickName : "",
                               FanName = users.Exists(x => x.ID == l.ToUserID) ? users.FirstOrDefault(x => x.ID == l.ToUserID).NickName : "",
                               CreateDate = l.CreateDate.ToString("yyyy-MM-dd hh:mm:ss"),
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
        public ActionResult Edit()
        {
            var callback = ZNRequest.GetString("jsoncallback");
            try
            {
                User user = GetUserInfo();
                if (user == null)
                {
                    return Content(callback + "(" + JsonConvert.SerializeObject(new { result = false, message = "用户信息验证失败" }) + ")");
                }
                var userID = ZNRequest.GetInt("ToUserID");
                if (userID == 0)
                {
                    return Content(callback + "(" + JsonConvert.SerializeObject(new { result = false, message = "信息异常,请刷新重试" }) + ")");
                }
                Fan model = db.Single<Fan>(x => x.FromUserID == user.ID && x.ToUserID == userID);
                if (model == null)
                {
                    model = new Fan();
                    model.FromUserID = user.ID;
                    model.ToUserID = userID;
                    model.Status = Enum_Status.Approved;
                }
                model.CreateDate = DateTime.Now;
                model.CreateIP = Tools.GetClientIP;
                var result = false;
                if (model.ID == 0)
                {
                    result = Tools.SafeInt(db.Add<Fan>(model)) > 0;
                }
                else
                {
                    result = db.Update<Fan>(model) > 0;
                }
                if (result)
                {
                    return Content(callback + "(" + JsonConvert.SerializeObject(new { result = true, message = "成功" }) + ")");
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error(ex.Message);
            }
            return Content(callback + "(" + JsonConvert.SerializeObject(new { result = false, message = "失败" }) + ")");
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
            var model = db.Single<Fan>(x => x.ID == id);
            try
            {
                if (model != null)
                {
                    model.Status = Enum_Status.DELETE;
                    result = db.Update<Fan>(model) > 0;
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
            var callback = ZNRequest.GetString("jsoncallback");
            try
            {
                var pager = new Pager();
                var query = new SubSonic.Query.Select(Repository.GetProvider()).From<Fan>().Where<Fan>(x => x.Status == Enum_Status.Approved);

                //关注人
                var FromUserID = ZNRequest.GetInt("FromUserID");
                if (FromUserID > 0)
                {
                    query = query.And("FromUserID").IsEqualTo(FromUserID);
                }

                //被关注人
                var ToUserID = ZNRequest.GetInt("ToUserID");
                if (ToUserID > 0)
                {
                    query = query.And("ToUserID").IsEqualTo(ToUserID);
                }
                var recordCount = query.GetRecordCount();
                var totalPage = recordCount % pager.Size == 0 ? recordCount / pager.Size : recordCount / pager.Size + 1;
                var list = query.Paged(pager.Index, pager.Size).OrderDesc("ID").ExecuteTypedList<Fan>();
                var array = new List<int>();
                if (FromUserID > 0)
                {
                    array = list.Select(x => x.ToUserID).Distinct().ToList();
                }
                if (ToUserID > 0)
                {
                    array = list.Select(x => x.FromUserID).Distinct().ToList();
                }
                var users = new SubSonic.Query.Select(Repository.GetProvider(), "ID", "NickName", "Avatar", "Signature").From<User>().Where<User>(x => x.Status == Enum_Status.Approved).And("ID").In(array.ToArray()).ExecuteTypedList<User>();

                //我关注的列表
                if (FromUserID > 0)
                {
                    var newlist = (from l in list
                                   join u in users on l.ToUserID equals u.ID
                                   select new
                                   {
                                       CreateDate = l.CreateDate.ToString("yyyy-MM-dd"),
                                       NickName = u.NickName,
                                       Signature = u.Signature,
                                       Avatar = GetFullUrl(u.Avatar)
                                   }).ToList();
                    var result = new
                    {
                        currpage = pager.Index,
                        records = recordCount,
                        totalpage = totalPage,
                        list = newlist
                    };
                    var message = callback + "(" + Newtonsoft.Json.JsonConvert.SerializeObject(result) + ")";
                    return Content(message);
                }

                //关注我的列表
                if (ToUserID > 0)
                {
                    var newlist = (from l in list
                                   join u in users on l.FromUserID equals u.ID
                                   select new
                                   {
                                       CreateDate = l.CreateDate.ToString("yyyy-MM-dd"),
                                       NickName = u.NickName,
                                       Signature = u.Signature,
                                       Avatar = GetFullUrl(u.Avatar)
                                   }).ToList();
                    var result = new
                    {
                        currpage = pager.Index,
                        records = recordCount,
                        totalpage = totalPage,
                        list = newlist
                    };
                    var message = callback + "(" + Newtonsoft.Json.JsonConvert.SerializeObject(result) + ")";
                    return Content(message);
                }
                return Content(callback + "()");
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error(ex.Message);
                return Content(callback + "()");
            }
        }

        #endregion
    }
}
