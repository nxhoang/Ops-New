using OPS_DAL.Business;
using OPS_DAL.Entities;
using OPS_Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OPS.Controllers
{
    public class UploadController : Controller
    {
        [HttpPost]
        public string UploadImage(string type, string id)
        {
            string sysFileName = "";
            string fullName = "";
            foreach (string file in Request.Files)
            {
                var fileContent = Request.Files[file];
                var data = Request.Form;
                var fileName = fileContent.FileName;
                var extFile = Path.GetExtension(fileName);
                sysFileName = id + extFile;
                //sysFileName = GetFileName(extFile);
                fullName = SaveImage(fileContent, GetPath(type), sysFileName);
                return sysFileName;

            }
            return sysFileName;
        }

        public string SaveImage(HttpPostedFileBase fileContent, string path, string fileName)
        {
            var sysFileName = path + fileName;
            if (System.IO.File.Exists(sysFileName))
            {
                System.IO.File.Delete(sysFileName);
            }
            fileContent.SaveAs(sysFileName);
            return sysFileName;
        }
        string GetFileName(string extFile)
        {
            string newName = DateTime.Now.ToUniversalTime().ToString("yyMMddhhmmssms");
            return newName + extFile;
        }
        string GetPath(string type)
        {
            string path = "";
            switch (type)
            {
                case "1": // machine
                     path = ConfigurationManager.AppSettings["OperationToolsImageDirectory"];
                    break;
                default:
                    path = "";
                    break;
            }
            return path;
        }

        [HttpPost]
        public bool UploadImageFPT()
        {
            Usmt UserInf = (Usmt)Session["LoginUser"];
            HttpPostedFileBase fileContent;
            foreach (string f in Request.Files)
            {
                fileContent = Request.Files[f];
                var fileName = fileContent.FileName;
                var extFile = Path.GetExtension(fileName);
                var urlPath = "/BETAPDM/User/" + UserInf.UserName + extFile;
                MemoryStream target = new MemoryStream();
                fileContent.InputStream.CopyTo(target);
                byte[] data = target.ToArray();
                return UploadFileToFtpFromClient(data, urlPath);
            }
            return false;
        }

        bool UploadFileToFtpFromClient(byte[] files, string urlPath)
        {
            
            FtpInfoBus f = new FtpInfoBus();
            FtpInfo fo = FtpInfoBus.GetFtpInfo("PKPMAIN");
            fo.FtpRoot = fo.FtpLink + "/BETAPDM/";
            return f.UploadFTP(files, fo.FtpLink + urlPath, fo.FtpUser, fo.FtpPass);
        }
    }
}