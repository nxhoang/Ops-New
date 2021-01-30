using Google.Protobuf.Collections;
using MES.CommonClass;
using OfficeOpenXml;
using OPS_DAL.MesBus;
using OPS_DAL.MesEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MES.Controllers
{
    public class IotMesController : Controller
    {
        McmpBus _mcmpBus = new McmpBus();
        public IotMesController()
        {

        }

        //[SysActionFilter(SystemID = "MES", MenuID = "IOT", Action = "")]
        public ActionResult MachineIot()
        {
            return View();
        }

        public async Task<ActionResult> UploadMapping()
        {
            /* Created by Tai Le (Thomas)
             * Create on 2019-09-26
             */
            try
            {
                string strAccumMsg = "";
                foreach (string fileName in Request.Files)
                {
                    HttpPostedFileBase file = Request.Files[fileName];
                    System.IO.Stream documentConverted = file.InputStream;
                    ExcelPackage package = new ExcelPackage(documentConverted);
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.FirstOrDefault();
                    // get number of rows and columns in the sheet
                    int rows = worksheet.Dimension.Rows; // 20
                    int columns = worksheet.Dimension.Columns; // 7 

                    //Data
                    // loop through the worksheet rows
                    List<Mcmp> mcmpLst = new List<Mcmp>();
                    for (int i = 2; i <= rows; i++)
                    {
                        var tempObj = new Mcmp();
                        //MAC:  
                        if (worksheet.Cells[i, 1].Value != null)
                        {
                            tempObj.IOT_MAC = worksheet.Cells[i, 1].Value.ToString();
                        }
                        // Machine ID
                        if (worksheet.Cells[i, 2].Value != null)
                        {
                            tempObj.MACHINE_ID = worksheet.Cells[i, 2].Value.ToString();
                        }
                        //Position
                        if (worksheet.Cells[i, 3].Value != null)
                        {
                            tempObj.IOT_POSITION = worksheet.Cells[i, 3].Value.ToString();
                        }
                        mcmpLst.Add(tempObj);
                    }

                    worksheet.Dispose();
                    package.Dispose();

                    int SuccessCtn = 0, FailCtn = 0;
                    foreach (var item in mcmpLst)
                    {
                        var res = await _mcmpBus.UpdateMachineIotMapping(item);
                        if (res == "OK")
                        {
                            SuccessCtn++;
                        }
                        else
                            FailCtn++;
                    }
                    strAccumMsg = $"-Success: {SuccessCtn}<br/> -Fail: {FailCtn}";

                }
                return Json(new { retResult = true, retMsg = $"Upload Result:<br/>{strAccumMsg}" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { retResult = false, retMsg = "ERROR:<BR/>" + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}