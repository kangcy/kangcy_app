using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using CommonTools;

namespace EGT_OTA.Controllers
{
    public class UploadController : Controller
    {
        public ActionResult UploadFile()
        {
            var count = Request.Files.Count;
            if (count == 0)
            {
                return this.HttpNotFound();
            }

            string data = DateTime.Now.ToString("yyyy-MM-dd");
            string virtualPath = "~/Upload/" + data;
            string savePath = this.Server.MapPath(virtualPath);
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }
            List<string> list = new List<string>();
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
            return Content(string.Join(",", list.ToArray()));
        }
    }
}
