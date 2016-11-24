using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using CommonTools;
using EGT_OTA.Helper;
using System.Drawing;
using Newtonsoft.Json;

namespace EGT_OTA.Controllers
{
    /// <summary>
    /// 上传文件
    /// </summary>
    public class UploadController : Controller
    {
        public static string TxtExtensions = ",doc,docx,docm,dotx,txt,xml,htm,html,mhtml,wps,";
        public static string XlsExtensions = ",xls,xlsm,xlsb,xlsm,";
        public static string ImageExtensions = ",jpg,jpeg,jpe,png,gif,bmp,";
        public static string CompressionExtensions = ",zip,rar,";
        public static string AudioExtensions = ",mp3,wav,";
        public static string VideoExtensions = ",mp4,avi,wmv,mkv,3gp,flv,rmvb,";

        public ActionResult UploadFile()
        {
            var result = false;
            var message = string.Empty;
            var count = Request.Files.Count;
            if (count == 0)
            {
                return Json(new { result = result, message = "未上传任何文件" }, JsonRequestBehavior.AllowGet);
            }

            var folder = ZNRequest.GetString("folder");

            var file = Request.Files[0];
            string extension = Path.GetExtension(file.FileName);

            if (string.IsNullOrWhiteSpace(folder))
            {
                folder = "Other";
            }
            else
            {
                if (folder.ToLower() == "pic" && !ImageExtensions.Contains(extension.ToLower().Replace(".", "")))
                {
                    return Json(new { result = false, message = "上传文件格式不正确" }, JsonRequestBehavior.AllowGet);
                }
                if (folder.ToLower() == "music" && !AudioExtensions.Contains(extension.ToLower().Replace(".", "")))
                {
                    return Json(new { result = false, message = "上传文件格式不正确" }, JsonRequestBehavior.AllowGet);
                }
                if (folder.ToLower() == "video" && !VideoExtensions.Contains(extension.ToLower().Replace(".", "")))
                {
                    return Json(new { result = false, message = "上传文件格式不正确" }, JsonRequestBehavior.AllowGet);
                }
            }
            var url = string.Empty;
            try
            {
                string data = DateTime.Now.ToString("yyyy-MM-dd");
                string virtualPath = "~/Upload/" + folder + "/" + data;
                string savePath = this.Server.MapPath(virtualPath);
                if (!Directory.Exists(savePath))
                {
                    Directory.CreateDirectory(savePath);
                }
                string filename = Path.GetFileName(file.FileName);
                string code = DateTime.Now.ToString("yyyyMMddHHmmss") + new Random().Next(10000);
                string fileExtension = Path.GetExtension(filename);//获取文件后缀名(.jpg)
                filename = code + fileExtension;//重命名文件
                savePath = savePath + "\\" + filename;
                file.SaveAs(savePath);
                url = Url.Content(virtualPath + "/" + filename);
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error("UploadController_UploadFile" + ex.Message, ex);
                message = ex.Message;
            }
            return Json(new { result = true, message = url }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Upload()
        {
            var error = string.Empty;
            try
            {
                string data = DateTime.Now.ToString("yyyy-MM-dd");
                string virtualPath = "Upload/images/" + data;
                string savePath = this.Server.MapPath("~/" + virtualPath);
                if (!Directory.Exists(savePath))
                {
                    Directory.CreateDirectory(savePath);
                }
                string stream = ZNRequest.GetString("str");

                stream = stream.IndexOf("data:image/jpeg;base64,") > -1 ? stream.Replace("data:image/jpeg;base64,", "") : stream;

                string filename = Guid.NewGuid().ToString("N") + ".jpg";

                savePath = savePath + "\\" + filename;

                Byte[] streamByte = Convert.FromBase64String(stream);
                System.IO.MemoryStream ms = new System.IO.MemoryStream(streamByte);
                Image image = Image.FromStream(ms, true);
                image.Save(savePath, System.Drawing.Imaging.ImageFormat.Jpeg);
                image.Dispose();

                return Json(new
                {
                    result = true,
                    message = (virtualPath + "/" + filename)
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                LogHelper.ErrorLoger.Error("UploadController_Upload" + ex.Message, ex);
            }
            return Json(new
            {
                result = false,
                message = error
            }, JsonRequestBehavior.AllowGet);
        }
    }
}
