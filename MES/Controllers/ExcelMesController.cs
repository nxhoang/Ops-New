using MES.CommonClass;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using OPS_DAL.MesBus;
using System;
using System.Drawing;
using System.IO;
using System.Web.Mvc;

namespace MES.Controllers
{
    public class ExcelMesController : Controller
    {
        // GET: ExcelExport
        public ActionResult ExcelMes()
        {
            return View();
        }

        enum WPColumn
        {
            Assembly = 1,
            MesPackage,
            Factory,
            ModuleName,
            ModuleID,
            OpGroup,
            Target,
            TotalMade,
            Issued,
            Balance,
            Shipped
        }

        private const string ExcelColWPStart = "A";
        private const string ExcelColWPEnd = "K";

        public JsonResult ExportWorkingProcess(string mesPackage, string getType)
        {
            try
            {
                //Check MES package is empty
                if (string.IsNullOrEmpty(mesPackage))
                {
                    return Json(new { fileName = "", errorMessage = "MES package is empty" }, JsonRequestBehavior.AllowGet);
                }

                //Get list of process
                //Get style information in mes package
                var styleInf = mesPackage.Split('_')[3];
                var styleCode = styleInf.Substring(0, 7);
                var styleSize = styleInf.Substring(7, 3);
                var styleColorSerial = styleInf.Substring(10, 3);
                var revNo = styleInf.Substring(13, 3);
                var wkProcess = MpdtBus.GetMesPackageWorkingProcess(mesPackage, styleCode, styleSize, styleColorSerial, revNo, getType);

                //Get object final assembly
                var finalAssembly = wkProcess.Find(x => x.IoTType == "FA");
                //Get completed quantity of Final Assembly process
                var issuedQty = finalAssembly == null ? 0 : finalAssembly.TotalMade;

                //Update issued quantity for Sub Assembly
                foreach (var wp in wkProcess)
                {
                    if(wp.IoTType != "FA")
                    {
                        wp.Issued = issuedQty;
                    }
                }

                //Init start row and end row to fill data
                var startRow = 2;
                var lastRow = 1;

                //Create file name with current date time
                var curDate = DateTime.Now.ToString("yyyyMMddhhmmss");
                var newFileName = "work-in-process-" + curDate + ".xlsx";

                //Create new path file
                var newFilePath = Path.Combine(Server.MapPath("~/temp"), newFileName);

                var newFileInfo = new FileInfo(newFilePath);
                //using (var source = System.IO.File.OpenRead(tempFilePath))
                using (var excelPackage = new ExcelPackage())
                {
                    //Add worksheet
                    excelPackage.Workbook.Worksheets.Add("work-in-process");
                    // Open worksheet 1
                    ExcelWorksheet excelWorkSheet = excelPackage.Workbook.Worksheets[1];

                    //Create header name
                    excelWorkSheet.Cells[1, (int)WPColumn.Assembly].Value = "Assembly";
                    excelWorkSheet.Cells[1, (int)WPColumn.MesPackage].Value = "MES Pakcage";
                    excelWorkSheet.Cells[1, (int)WPColumn.Factory].Value = "Factory";
                    excelWorkSheet.Cells[1, (int)WPColumn.ModuleName].Value = "Module Name";
                    excelWorkSheet.Cells[1, (int)WPColumn.ModuleID].Value = "Module ID";
                    excelWorkSheet.Cells[1, (int)WPColumn.OpGroup].Value = "OpGroup";
                    excelWorkSheet.Cells[1, (int)WPColumn.Target].Value = "Target";
                    excelWorkSheet.Cells[1, (int)WPColumn.Issued].Value = "Issued";
                    excelWorkSheet.Cells[1, (int)WPColumn.TotalMade].Value = "Total Made";
                    excelWorkSheet.Cells[1, (int)WPColumn.Balance].Value = "Balance";
                    excelWorkSheet.Cells[1, (int)WPColumn.Shipped].Value = "Shipped";

                    //Format for header
                    Color colFromHex = ColorTranslator.FromHtml("#B7DEE8");
                    excelWorkSheet.Cells[ExcelColWPStart + "1:" + ExcelColWPEnd + "1"].Style.Font.Bold = true;
                    excelWorkSheet.Cells[ExcelColWPStart + "1:" + ExcelColWPEnd + "1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    excelWorkSheet.Cells[ExcelColWPStart + "1:" + ExcelColWPEnd + "1"].Style.Fill.BackgroundColor.SetColor(colFromHex);

                    //Create data for exporting excel
                    for (int j = 0; j < wkProcess.Count; j++) //Loop for row.
                    {
                        excelWorkSheet.Cells[j + startRow, (int)WPColumn.Assembly].Value = wkProcess[j].IoTType;
                        excelWorkSheet.Cells[j + startRow, (int)WPColumn.MesPackage].Value = wkProcess[j].MxPackage;
                        excelWorkSheet.Cells[j + startRow, (int)WPColumn.Factory].Value = wkProcess[j].Factory;
                        excelWorkSheet.Cells[j + startRow, (int)WPColumn.ModuleName].Value = wkProcess[j].ModuleName;
                        excelWorkSheet.Cells[j + startRow, (int)WPColumn.ModuleID].Value = wkProcess[j].ModuleId;
                        excelWorkSheet.Cells[j + startRow, (int)WPColumn.OpGroup].Value = wkProcess[j].OpGroupName;
                        excelWorkSheet.Cells[j + startRow, (int)WPColumn.Target].Value = wkProcess[j].MxTarget;
                        excelWorkSheet.Cells[j + startRow, (int)WPColumn.Issued].Value = wkProcess[j].Issued;
                        excelWorkSheet.Cells[j + startRow, (int)WPColumn.TotalMade].Value = wkProcess[j].TotalMade;
                        excelWorkSheet.Cells[j + startRow, (int)WPColumn.Balance].Value = wkProcess[j].Balance;
                        excelWorkSheet.Cells[j + startRow, (int)WPColumn.Shipped].Value = wkProcess[j].Shipped;

                        lastRow = j;
                    }

                    //Calculate end row
                    int endRow = startRow + lastRow;

                    //Color border cell
                    using (ExcelRange range = excelWorkSheet.Cells[ExcelColWPStart + (startRow - 1).ToString() + ":" + ExcelColWPEnd + endRow])
                    {
                        range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Top.Color.SetColor(Color.Black);
                        range.Style.Border.Bottom.Color.SetColor(Color.Black);
                        range.Style.Border.Left.Color.SetColor(Color.Black);
                        range.Style.Border.Right.Color.SetColor(Color.Black);
                    }

                    //Auto fix column entire worksheet
                    excelWorkSheet.Cells[excelWorkSheet.Dimension.Address].AutoFitColumns();

                    //Save excel file
                    FileInfo excelFile = new FileInfo(newFilePath);
                    excelPackage.SaveAs(excelFile);
                }

                //return the Excel file name
                return Json(new { fileName = newFileName, errorMessage = "" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { fileName = "", errorMessage = ex.Message }, JsonRequestBehavior.AllowGet); ;
            }


        }

        [HttpGet]
        [DeleteMesFile] //Action Filter, it will auto delete the file after download,                             
        public ActionResult Download(string file)
        {
            //get the temp folder and file path in server
            string fullPath = Path.Combine(Server.MapPath("~/temp"), file);

            //return the file for download, this is an Excel 
            //so I set the file content type to "application/vnd.ms-excel"
            return File(fullPath, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", file);
        }

    }
}