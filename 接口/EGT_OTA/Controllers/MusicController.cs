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
    /// 音乐管理
    /// </summary>
    public class MusicController : BaseController
    {
        /// <summary>
        /// 列表
        /// </summary>
        public ActionResult All()
        {
            try
            {
                var pager = new Pager();
                var query = new SubSonic.Query.Select(Repository.GetProvider()).From<Music>().Where<Music>(x => x.Status == Enum_Status.Approved);
                string Name = ZNRequest.GetString("Name");
                if (!string.IsNullOrWhiteSpace(Name))
                {
                    query = query.And("Name").Like("%" + Name + "%").Or("Author").Like("%" + Name + "%");
                }
                var recordCount = query.GetRecordCount();
                var totalPage = recordCount % pager.Size == 0 ? recordCount / pager.Size : recordCount / pager.Size + 1;
                var list = query.Paged(pager.Index, pager.Size).OrderDesc("ID").ExecuteTypedList<Music>();
                var newlist = (from l in list
                               select new
                               {
                                   ID = l.ID,
                                   Name = l.Name,
                                   Author = l.Author,
                                   Cover = GetFullUrl(l.Cover),
                                   FileUrl = GetFullUrl(l.FileUrl)
                               }).ToList();
                var result = new
                {
                    currpage = pager.Index,
                    records = recordCount,
                    totalpage = totalPage,
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
