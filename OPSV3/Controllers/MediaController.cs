using OPS_Utils;
using System.Configuration;
using System.IO;
using System.Net.Http;
using System.Web.Mvc;

namespace OPS.Controllers
{
    [SessionTimeout]
    public class MediaController : Controller
    {
        // GET: Media
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Play()
        {
            return View();
        }

        [HttpPost]
        public HttpResponseMessage HttpUploadFile()
        {
            //HttpRequestBase

            foreach (string file in Request.Files)
            {
                var FileDataContent = Request.Files[file];
                if (FileDataContent != null && FileDataContent.ContentLength > 0)
                {
                    // take the input stream, and save it to a temp folder using the original file.part name posted
                    var stream = FileDataContent.InputStream;

                    //var fileName = Path.GetFileName(FileDataContent.FileName);
                    var SendKeyValues = FileDataContent.FileName.Split('|');
                    var fileName = SendKeyValues[0].ToString();
                    var Company = SendKeyValues[1].ToString();
                    var Team = SendKeyValues[2].ToString();
                    var FileComment = SendKeyValues[3].ToString();

                    var lsChBPublic = SendKeyValues[4].ToString();
                    var lsChBOnlyDepartmen = SendKeyValues[5].ToString();
                    var lsChBPrivate = SendKeyValues[6].ToString();

                    var UploadPath = Server.MapPath(ConfigurationManager.AppSettings["OpsVideoPath"] + Company + Team);
                    Directory.CreateDirectory(UploadPath);

                    string path = Path.Combine(UploadPath, fileName);
                    try
                    {
                        if (System.IO.File.Exists(path)) System.IO.File.Delete(path);

                        using (var fileStream = System.IO.File.Create(path))
                        {
                            stream.CopyTo(fileStream);
                        }

                        // Once the file part is saved, see if we have enough to merge it
                        WebUtils UT = new WebUtils();
                        UT.MergeFile(path, Company, Team, FileComment, "Tuong", lsChBPublic, lsChBOnlyDepartmen, lsChBPrivate, "N");
                    }
                    catch
                    {
                        // handle
                    }
                }
            }
            return new HttpResponseMessage()
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent("File uploaded Sucessful.")
            };
        }
        //[HttpPost]
        public string UploadFile()
        {
            string kq = "";

            foreach (string file in Request.Files)
            {
                var FileDataContent = Request.Files[file];
                if (FileDataContent != null && FileDataContent.ContentLength > 0)
                {
                    // take the input stream, and save it to a temp folder using the original file.part name posted
                    var stream = FileDataContent.InputStream;
                    //var fileName = Path.GetFileName(FileDataContent.FileName);
                    var SendKeyValues = FileDataContent.FileName.Split('|');
                    var fileName = SendKeyValues[0].ToString();
                    var Company = SendKeyValues[1].ToString();
                    var Team = SendKeyValues[2].ToString();
                    var UploadPath = Server.MapPath(ConfigurationManager.AppSettings["OpsVideoPath"] + Company + Team);
                    Directory.CreateDirectory(UploadPath);
                    string path = Path.Combine(UploadPath, fileName);
                    try
                    {
                        if (System.IO.File.Exists(path))
                            System.IO.File.Delete(path);
                        using (var fileStream = System.IO.File.Create(path))
                        {
                            stream.CopyTo(fileStream);
                        }
                        // Once the file part is saved, see if we have enough to merge it
                        // if last it will do
                        WebUtils UT = new WebUtils();
                        kq = UT.MergeFile(path, Company, Team);
                    }
                    catch
                    {
                        // handle
                    }
                }
            }
            return kq;
        }
    }
}