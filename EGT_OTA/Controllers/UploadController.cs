using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using CommonTools;
using EGT_OTA.Helper;

namespace EGT_OTA.Controllers
{
    /// <summary>
    /// 上传文件
    /// </summary>
    public class UploadController : Controller
    {
        public ActionResult UploadFile()
        {
            var result = false;
            var message = string.Empty;
            var count = Request.Files.Count;
            if (count == 0)
            {
                return Json(new { result = result, message = "未上传任何文件" }, JsonRequestBehavior.AllowGet);
            }
            List<string> list = new List<string>();
            try
            {
                string data = DateTime.Now.ToString("yyyy-MM-dd");
                string virtualPath = "~/Upload/" + data;
                string savePath = this.Server.MapPath(virtualPath);
                if (!Directory.Exists(savePath))
                {
                    Directory.CreateDirectory(savePath);
                }
                for (var i = 0; i < count; i++)
                {
                    var file = Request.Files[i];
                    string filename = Path.GetFileName(file.FileName);
                    string code = DateTime.Now.ToString("yyyyMMddHHmmss") + new Random().Next(10000);
                    string fileExtension = Path.GetExtension(filename);//获取文件后缀名(.jpg)
                    filename = code + fileExtension;//重命名文件
                    savePath = savePath + "\\" + filename;
                    file.SaveAs(savePath);
                    list.Add(Url.Content(virtualPath + "/" + filename));
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error(ex.Message, ex);
                message = ex.Message;
            }
            return Json(new { result = true, message = string.Join(",", list.ToArray()) }, JsonRequestBehavior.AllowGet);
        }
    }
}
