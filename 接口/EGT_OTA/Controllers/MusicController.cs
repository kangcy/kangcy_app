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
        /// 类型列表
        /// </summary>
        public ActionResult TypeAll()
        {
            try
            {
                var list = new SubSonic.Query.Select(Repository.GetProvider()).From<ArticleType>().Where<MusicType>(x => x.Status == Enum_Status.Approved).OrderAsc("SortID").ExecuteTypedList<MusicType>();
                var newlist = (from l in list
                               select new
                               {
                                   ID = l.ID,
                                   Name = l.Name,
                                   CurrID = l.CurrID
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

        /// <summary>
        /// 列表
        /// </summary>
        public ActionResult All()
        {
            try
            {
                var musicType = db.Find<MusicType>(x => x.Status == Enum_Status.Approved).OrderBy(x => x.SortID).ToList();
                var music = db.All<Music>().ToList();
                musicType.ForEach(x =>
                {
                    x.Music = music.FindAll(y => y.TypeID == x.CurrID);
                    x.Music.ForEach(l =>
                    {
                        l.Cover = GetFullUrl(l.Cover);
                        l.FileUrl = GetFullUrl(l.FileUrl);
                    });
                });
                var result = new
                {
                    currpage = 1,
                    records = musicType.Count,
                    totalpage = 1,
                    list = musicType
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
