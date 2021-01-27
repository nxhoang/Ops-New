using OPS_DAL.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MES.Controllers
{
    [SessionTimeout]
    public class FileSyncController : Controller
    {
        // GET: FileSync
        public ActionResult Sync()
        {
            ViewBag.PageTitle = "File Synchronization";
            return View();
        }

        public JsonResult SyncFile(string buyer, string ao, string style, string overwrite, List<string> countryList, List<string> fileTypeList)
        {
            try
            {
                if (string.IsNullOrEmpty(buyer) || string.IsNullOrEmpty(ao))
                {
                    return Json("Please select Buyer", JsonRequestBehavior.AllowGet);
                }

                var listStyle = FileSdBus.GetFilesByAo(buyer, ao, style, fileTypeList);//AdsmBus.GetListStyleCodeByBuyer(buyer, ao, style);

                //var ftp = FtpInfoBus.GetFtpInfo();

                foreach (var fileType in fileTypeList)
                {
                    System.Threading.Thread.Sleep(3000);
                }

                return Json(listStyle.Count + " file(s)", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

    }
}