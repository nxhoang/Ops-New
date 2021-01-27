using OfficeOpenXml;
using OfficeOpenXml.Drawing;
using OfficeOpenXml.Drawing.Chart;
using OfficeOpenXml.Style;
using OPS_DAL.Business;
using OPS_DAL.Entities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Web;
using System.Web.Mvc;

using System.Net;
using OfficeOpenXml.FormulaParsing.ExpressionGraph.CompileStrategy;

namespace OPS.Controllers
{
    public class ExportExcelController : Controller
    {
        // GET: ExportExcel
        public ActionResult ExportExcel()
        {
            return View();
        }

        [HttpGet]
        public ActionResult ExportBalancingToExcel(string styleCode, string styleSize, string styleColorSerial, string revNo, string opRevNo, string edition, string languageId)
        {
            var curDate = DateTime.Now.ToString("yyyyMMddhhmmss");
            var newFileName = "line-balancing-" + curDate + ".xlsx";
            // Create excel file
            //var stream = CreateExcelFile(styleCode, styleSize, styleColorSerial, revNo, opRevNo, edition, languageId);

            var stream = CreateLineBalacingExcel(styleCode, styleSize, styleColorSerial, revNo, opRevNo, edition, languageId);

            // Create stream buffer
            var buffer = stream as MemoryStream;
            // Content type of file
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            // Show dialgo Save As when run on IE or Firefox
            Response.AddHeader("Content-Disposition", "attachment; filename=" + newFileName);
            // Save excel as binary array
            Response.BinaryWrite(buffer.ToArray());
            // Send all out put bytes to client
            Response.Flush();
            Response.End();
            // Redirect
            return RedirectToAction("ExportExcel");
        }

        private Stream CreateExcelFile(string styleCode, string styleSize, string styleColorSerial, string revNo, string opRevNo, string edition, string languageId)
        {
            //Get list of process
            var lstOpdt = OpdtBus.GetOpDetailByLanguage(styleCode, styleSize, styleColorSerial, revNo, opRevNo, edition, languageId);
            var startRow = 42;
            var endRow = 42;
            const string topLeft = "D42";
            string bottomRight = "";
            var lastRow = 0;
            const string graphTitle = "Line Balancing";
            const string xAxis = "Process Name ";
            const string yAxis = "Process Time";

            var tempFilePath = Server.MapPath("~/assets/excel-files/templates/line-balancing/Line-Balancing.xlsx");
            var newFilePath = Server.MapPath("~/assets/excel-files/exporting/line-balancing.xlsx");
            var templateFileInfo = new FileInfo(tempFilePath);
            var newFileInfo = new FileInfo(newFilePath);
            using (var source = System.IO.File.OpenRead(tempFilePath))
            using (var excelPackage = new ExcelPackage(newFileInfo, templateFileInfo))
            {
                // Open worksheet 1
                ExcelWorksheet excelWorkSheet = excelPackage.Workbook.Worksheets[1];

                //Set value for style
                excelWorkSheet.Cells[5, 4].Value = styleCode;
                excelWorkSheet.Cells[6, 4].Value = styleSize;
                excelWorkSheet.Cells[7, 4].Value = styleColorSerial;
                excelWorkSheet.Cells[8, 4].Value = revNo;
                excelWorkSheet.Cells[9, 4].Value = opRevNo;

                //Create data for exporting excel
                for (int j = 0; j < lstOpdt.Count; j++) //Loop for row.
                {
                    excelWorkSheet.Cells[j + startRow, 2].Value = lstOpdt[j].OpSerial;
                    excelWorkSheet.Cells[j + startRow, 3].Value = lstOpdt[j].OpNum;
                    excelWorkSheet.Cells[j + startRow, 4].Value = lstOpdt[j].OpName;
                    excelWorkSheet.Cells[j + startRow, 5].Value = lstOpdt[j].OpTime;
                    excelWorkSheet.Cells[j + startRow, 6].Value = lstOpdt[j].OpPrice;
                    excelWorkSheet.Cells[j + startRow, 7].Value = lstOpdt[j].FactoryName;
                    excelWorkSheet.Cells[j + startRow, 8].Value = lstOpdt[j].ManCount;
                    excelWorkSheet.Cells[j + startRow, 9].Value = lstOpdt[j].MachineType;
                    excelWorkSheet.Cells[j + startRow, 10].Value = lstOpdt[j].MachineCount;
                    excelWorkSheet.Cells[j + startRow, 11].Value = lstOpdt[j].OfferOpPrice;
                    excelWorkSheet.Cells[j + startRow, 12].Value = lstOpdt[j].MaxTime;
                    excelWorkSheet.Cells[j + startRow, 13].Value = lstOpdt[j].BenchmarkTime;

                    lastRow = j;
                }
                endRow = startRow + lastRow;
                bottomRight = "E" + endRow; //End of row.

                //Drawing chart
                ExcelChart chart = excelWorkSheet.Drawings.AddChart(graphTitle, eChartType.ColumnClustered);
                chart.Title.Text = "Line Balacing Chart";
                chart.XAxis.Title.Text = xAxis;
                chart.YAxis.Title.Text = yAxis;

                //Position at line 10, column 1
                chart.SetPosition(10, 0, 1, 0);
                chart.SetSize(900, 500);

                var ser1 = chart.Series.Add("E" + startRow + ":" + bottomRight, topLeft + ":" + "D" + endRow);
                ser1.Header = "Process time";

                //Color border cell
                using (ExcelRange range = excelWorkSheet.Cells["B" + startRow + ":M" + endRow])
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

                excelPackage.Save();
                return excelPackage.Stream;
            }

        }

        private Stream CreateLineBalacingExcel(string styleCode, string styleSize, string styleColorSerial, string revNo, string opRevNo, string edition, string languageId)
        {
            var lstOpdt = OpdtBus.GetOpDetailByLanguage(styleCode, styleSize, styleColorSerial, revNo, opRevNo, edition, languageId);
            var startRow = 42;
            var endRow = 42;
            const string topLeft = "D41";
            string bottomRight = "";
            var lastRow = 0;
            const string graphTitle = "Line Balancing";
            const string xAxis = "Process Name ";
            const string yAxis = "Process Time";

            var streamExcel = new MemoryStream();
            using (var package = new ExcelPackage())
            {
                //Create worksheet
                var excelWorkSheet = package.Workbook.Worksheets.Add("Sheet1");

                excelWorkSheet.Cells["B3"].Value = "LINE BALANCING DATA";
                excelWorkSheet.Cells["B3"].Style.Font.Bold = true;
                excelWorkSheet.Cells["B3"].Style.Font.Size = 20;
                excelWorkSheet.Cells["B3"].Style.Font.Color.SetColor(Color.FromArgb(0, 112, 192));

                //Merge cell
                excelWorkSheet.Cells["B5:C5"].Merge = true;
                excelWorkSheet.Cells["B6:C6"].Merge = true;
                excelWorkSheet.Cells["B7:C7"].Merge = true;
                excelWorkSheet.Cells["B8:C8"].Merge = true;
                excelWorkSheet.Cells["B9:C9"].Merge = true;

                excelWorkSheet.Cells["B5"].Value = "STYLE CODE";
                excelWorkSheet.Cells["B6"].Value = "STYLE SIZE";
                excelWorkSheet.Cells["B7"].Value = "STYLE COLOR SERIAL";
                excelWorkSheet.Cells["B8"].Value = "STYLE REVISION";
                excelWorkSheet.Cells["B9"].Value = "OP REVISION";
                excelWorkSheet.Cells["B5:C9"].Style.Font.Bold = true;
                using (ExcelRange range = excelWorkSheet.Cells["B5:D9"])
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

                //Header of grid op detail.
                Color colFromHex = System.Drawing.ColorTranslator.FromHtml("#d8eef3");
                excelWorkSheet.Cells["B41:J41"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                excelWorkSheet.Cells["B41:J41"].Style.Font.Bold = true;
                excelWorkSheet.Cells["B41:J41"].Style.Fill.BackgroundColor.SetColor(colFromHex);
                excelWorkSheet.Cells["B41:J41"].Style.Font.Size = 13;
                excelWorkSheet.Cells["B41"].Value = "Op Serial";
                excelWorkSheet.Cells["C41"].Value = "Op Number";
                excelWorkSheet.Cells["D41"].Value = "Process Name";
                excelWorkSheet.Cells["E41"].Value = "Op Time";
                excelWorkSheet.Cells["F41"].Value = "Factory";
                excelWorkSheet.Cells["G41"].Value = "Workers";
                excelWorkSheet.Cells["H41"].Value = "Machine Type";
                excelWorkSheet.Cells["I41"].Value = "Machines";
                excelWorkSheet.Cells["J41"].Value = "Max Time";

                //excelWorkSheet.Column(2).AutoFit();
                excelWorkSheet.Column(3).AutoFit();
                excelWorkSheet.Column(4).Width = 50;
                excelWorkSheet.Column(5).AutoFit();
                excelWorkSheet.Column(6).AutoFit();
                excelWorkSheet.Column(7).AutoFit();
                excelWorkSheet.Column(8).AutoFit();
                excelWorkSheet.Column(9).AutoFit();
                excelWorkSheet.Column(10).AutoFit();

                //Set value for style
                excelWorkSheet.Cells[5, 4].Value = styleCode;
                excelWorkSheet.Cells[6, 4].Value = styleSize;
                excelWorkSheet.Cells[7, 4].Value = styleColorSerial;
                excelWorkSheet.Cells[8, 4].Value = revNo;
                excelWorkSheet.Cells[9, 4].Value = opRevNo;

                //Create data for exporting excel
                for (int j = 0; j < lstOpdt.Count; j++) //Loop for row.
                {
                    excelWorkSheet.Cells[j + startRow, 2].Value = lstOpdt[j].OpSerial;
                    excelWorkSheet.Cells[j + startRow, 3].Value = lstOpdt[j].OpNum;
                    excelWorkSheet.Cells[j + startRow, 4].Value = lstOpdt[j].OpName;
                    excelWorkSheet.Cells[j + startRow, 5].Value = lstOpdt[j].OpTime;
                    excelWorkSheet.Cells[j + startRow, 6].Value = lstOpdt[j].FactoryName;
                    excelWorkSheet.Cells[j + startRow, 7].Value = lstOpdt[j].ManCount;
                    excelWorkSheet.Cells[j + startRow, 8].Value = lstOpdt[j].MachineCount;
                    excelWorkSheet.Cells[j + startRow, 9].Value = lstOpdt[j].MachineType;
                    excelWorkSheet.Cells[j + startRow, 10].Value = lstOpdt[j].MaxTime;

                    lastRow = j;
                }
                endRow = startRow + lastRow;
                bottomRight = "E" + endRow; //End of row.

                //Drawing chart
                ExcelChart chart = excelWorkSheet.Drawings.AddChart(graphTitle, eChartType.ColumnClustered);
                chart.Title.Text = "Line Balacing Chart";
                chart.XAxis.Title.Text = xAxis;
                chart.YAxis.Title.Text = yAxis;

                //Position at line 10, column 1
                chart.SetPosition(10, 0, 1, 0);
                chart.SetSize(900, 500);

                var ser1 = chart.Series.Add("E" + startRow + ":" + bottomRight, topLeft + ":" + "D" + endRow);
                ser1.Header = "Process time";

                //Color border cell
                using (ExcelRange range = excelWorkSheet.Cells["B" + (startRow - 1) + ":J" + endRow])
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

                streamExcel = new MemoryStream(package.GetAsByteArray());
            }

            return streamExcel;
        }

        [HttpGet]
        public ActionResult ExportProcessDetailToExcel(string styleCode, string styleSize, string styleColorSerial, string revNo, string opRevNo, string edition, string languageId)
        {
            var curDate = DateTime.Now.ToString("yyyyMMddhhmmss");
            var newFileName = "process-detail-" + curDate + ".xlsx";
            // Create excel file
            //var stream = CreateExcelFileProcessDetail(styleCode, styleSize, styleColorSerial, revNo, opRevNo, edition, languageId);
            var stream = CreateExcelInStreamOpDetail(styleCode, styleSize, styleColorSerial, revNo, opRevNo, edition, languageId);

            // Create stream buffer
            var buffer = stream as MemoryStream;
            // Content type of file
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            // Show dialgo Save As when run on IE or Firefox
            Response.AddHeader("Content-Disposition", "attachment; filename=" + newFileName);
            // Save excel as binary array
            Response.BinaryWrite(buffer.ToArray());
            // Send all out put bytes to client
            Response.Flush();
            Response.End();
            // Redirect
            return RedirectToAction("ExportExcel");
        }

        private Stream CreateExcelFileProcessDetail(string styleCode, string styleSize, string styleColorSerial, string revNo, string opRevNo, string edition, string languageId)
        {
            //Get list of process
            var lstOpdt = OpdtBus.GetOpDetailByLanguage(styleCode, styleSize, styleColorSerial, revNo, opRevNo, edition, languageId);
            var startRow = 2;
            var endRow = 2;
            string bottomRight = "";
            var lastRow = 0;

            var tempFilePath = Server.MapPath("~/assets/excel-files/templates/process-detail/process-template.xlsx");
            var newFilePath = Server.MapPath("~/assets/excel-files/exporting/process-detail.xlsx");
            var templateFileInfo = new FileInfo(tempFilePath);
            var newFileInfo = new FileInfo(newFilePath);
            using (var source = System.IO.File.OpenRead(tempFilePath))
            using (var excelPackage = new ExcelPackage(newFileInfo, templateFileInfo))
            {
                // Open worksheet 1
                ExcelWorksheet excelWorkSheet = excelPackage.Workbook.Worksheets[1];

                //Create data for exporting excel
                for (int j = 0; j < lstOpdt.Count; j++) //Loop for row.
                {
                    excelWorkSheet.Cells[j + startRow, 1].Value = lstOpdt[j].OpSerial;
                    excelWorkSheet.Cells[j + startRow, 2].Value = lstOpdt[j].OpNum;
                    excelWorkSheet.Cells[j + startRow, 3].Value = lstOpdt[j].OpRevNo;
                    excelWorkSheet.Cells[j + startRow, 4].Value = lstOpdt[j].OpGroup;
                    excelWorkSheet.Cells[j + startRow, 5].Value = lstOpdt[j].OpNameLan;
                    excelWorkSheet.Cells[j + startRow, 6].Value = lstOpdt[j].OpTime;
                    excelWorkSheet.Cells[j + startRow, 7].Value = lstOpdt[j].MaxTime;
                    excelWorkSheet.Cells[j + startRow, 8].Value = lstOpdt[j].MachineName;
                    excelWorkSheet.Cells[j + startRow, 9].Value = lstOpdt[j].MachineCount;
                    excelWorkSheet.Cells[j + startRow, 10].Value = lstOpdt[j].ManCount;
                    excelWorkSheet.Cells[j + startRow, 11].Value = lstOpdt[j].OpPrice;
                    excelWorkSheet.Cells[j + startRow, 12].Value = lstOpdt[j].OfferOpPrice;
                    excelWorkSheet.Cells[j + startRow, 13].Value = lstOpdt[j].OpDesc;
                    excelWorkSheet.Cells[j + startRow, 14].Value = lstOpdt[j].Remarks;
                    excelWorkSheet.Cells[j + startRow, 15].Value = lstOpdt[j].Temperature;
                    excelWorkSheet.Cells[j + startRow, 16].Value = lstOpdt[j].DryingTime;
                    excelWorkSheet.Cells[j + startRow, 17].Value = lstOpdt[j].CoolingTime;
                    excelWorkSheet.Cells[j + startRow, 18].Value = lstOpdt[j].MaterialTypeName;
                    excelWorkSheet.Cells[j + startRow, 19].Value = lstOpdt[j].PaintingTypeName;

                    lastRow = j;
                }
                endRow = startRow + lastRow;
                bottomRight = "O" + endRow; //End of row.

                //Color border cell
                using (ExcelRange range = excelWorkSheet.Cells["A" + startRow + ":S" + endRow])
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

                excelPackage.Save();
                return excelPackage.Stream;
            }

        }

        private Stream CreateExcelInStreamOpDetail(string styleCode, string styleSize, string styleColorSerial, string revNo, string opRevNo, string edition, string languageId)
        {
            var ms = new MemoryStream();
            using(var ex = new ExcelPackage())
            {
               
                //Get list of process
                var lstOpdt = OpdtBus.GetOpDetailByLanguage(styleCode, styleSize, styleColorSerial, revNo, opRevNo, edition, languageId);
                var startRow = 2;
                var endRow = 2;
                string bottomRight = "";
                var lastRow = 0;

                //Create worksheet
                var ws = ex.Workbook.Worksheets.Add("Sheet1");

                //Create excel header
                ws.Cells["A1"].Value = "OPSerial";
                ws.Cells["B1"].Value = "OP Number";
                ws.Cells["C1"].Value = "OP RevNo";
                ws.Cells["D1"].Value = "OP Group";
                ws.Cells["E1"].Value = "OP Name";
                ws.Cells["F1"].Value = "OP Name Tranlation";
                ws.Cells["G1"].Value = "OP Time";
                ws.Cells["H1"].Value = "Max Time";
                ws.Cells["I1"].Value = "Machine Name";
                ws.Cells["J1"].Value = "Machine Count";
                ws.Cells["K1"].Value = "Man Count";
                ws.Cells["L1"].Value = "OpPrice";
                ws.Cells["M1"].Value = "OfferOpPrice";
                ws.Cells["N1"].Value = "OpDesc";
                ws.Cells["O1"].Value = "Remarks";
                ws.Cells["P1"].Value = "Temperature";
                ws.Cells["Q1"].Value = "Drying Time";
                ws.Cells["R1"].Value = "Cooling Time";
                ws.Cells["S1"].Value = "Material Type";
                ws.Cells["T1"].Value = "Painting Type";

                ws.Cells["A1:T1"].Style.Font.Bold = true;

                ws.Column(1).AutoFit();
                ws.Column(2).AutoFit();
                ws.Column(3).AutoFit();
                ws.Column(4).AutoFit();
                ws.Column(5).Width = 50;
                ws.Column(6).Width = 50;
                ws.Column(7).AutoFit();
                ws.Column(8).AutoFit();
                ws.Column(9).AutoFit();
                ws.Column(10).AutoFit();
                ws.Column(11).AutoFit();
                ws.Column(12).AutoFit();
                ws.Column(13).AutoFit();
                ws.Column(14).AutoFit();
                ws.Column(15).Width = 50;
                ws.Column(16).AutoFit();
                ws.Column(17).AutoFit();
                ws.Column(18).AutoFit();
                ws.Column(19).AutoFit();
                ws.Column(20).AutoFit();
                

                //Create data for exporting excel
                for (int j = 0; j < lstOpdt.Count; j++) //Loop for row.
                {
                    ws.Cells[j + startRow, 1].Value = lstOpdt[j].OpSerial;
                    ws.Cells[j + startRow, 2].Value = lstOpdt[j].OpNum;
                    ws.Cells[j + startRow, 3].Value = lstOpdt[j].OpRevNo;
                    ws.Cells[j + startRow, 4].Value = lstOpdt[j].OpGroup;
                    ws.Cells[j + startRow, 5].Value = lstOpdt[j].OpName;
                    ws.Cells[j + startRow, 6].Value = lstOpdt[j].OpNameLan;
                    ws.Cells[j + startRow, 7].Value = lstOpdt[j].OpTime;
                    ws.Cells[j + startRow, 8].Value = lstOpdt[j].MaxTime;
                    ws.Cells[j + startRow, 9].Value = lstOpdt[j].MachineName;
                    ws.Cells[j + startRow, 10].Value = lstOpdt[j].MachineCount;
                    ws.Cells[j + startRow, 11].Value = lstOpdt[j].ManCount;
                    ws.Cells[j + startRow, 12].Value = lstOpdt[j].OpPrice;
                    ws.Cells[j + startRow, 13].Value = lstOpdt[j].OfferOpPrice;
                    ws.Cells[j + startRow, 14].Value = lstOpdt[j].OpDesc;
                    ws.Cells[j + startRow, 15].Value = lstOpdt[j].Remarks;
                    ws.Cells[j + startRow, 16].Value = lstOpdt[j].Temperature;
                    ws.Cells[j + startRow, 17].Value = lstOpdt[j].DryingTime;
                    ws.Cells[j + startRow, 18].Value = lstOpdt[j].CoolingTime;
                    ws.Cells[j + startRow, 19].Value = lstOpdt[j].MaterialTypeName;
                    ws.Cells[j + startRow, 20].Value = lstOpdt[j].PaintingTypeName;

                    lastRow = j;
                }
                endRow = startRow + lastRow;
                bottomRight = "O" + endRow; //End of row.

                //Color border cell
                using (ExcelRange range = ws.Cells["A" + (startRow - 1) + ":T" + endRow])
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

                ms = new MemoryStream(ex.GetAsByteArray());
            }

            return ms;
        }

        //START ADD: HA
        [HttpGet]
        public ActionResult ExportMachineListToExcel(string styleCode, string styleSize, string styleColorSerial, string revNo, string opRevNo, string edition)
        {
            var curDate = DateTime.Now.ToString("yyyyMMddhhmmss");
            var newFileName = "machine-list-" + curDate + ".xlsx";
            // Create excel file
            var stream = CreateExcelFileMachineList(styleCode, styleSize, styleColorSerial, revNo, opRevNo, edition);
            // Create stream buffer
            var buffer = stream as MemoryStream;
            // Content type of file
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            // Show dialgo Save As when run on IE or Firefox
            Response.AddHeader("Content-Disposition", "attachment; filename=" + newFileName);
            // Save excel as binary array
            Response.BinaryWrite(buffer.ToArray());
            // Send all out put bytes to client
            Response.Flush();
            Response.End();
            // Redirect
            return RedirectToAction("ExportExcel");
        }

        private Stream CreateExcelFileMachineList(string styleCode, string styleSize, string styleColorSerial, string revNo, string opRevNo, string edition)
        {
            try
            {


                //Get list of machine
                var lstOptl = OptlBus.GetMachineJquery(styleCode, styleSize, styleColorSerial, revNo, opRevNo, edition);
                var startRow = 2;
                var endRow = 2;
                string bottomRight = "";
                var lastRow = 0;

                var tempFilePath = Server.MapPath("~/assets/excel-files/machine/machine-list.xlsx");
                var newFilePath = Server.MapPath("~/assets/excel-files/exporting/machine-list.xlsx");
                var templateFileInfo = new FileInfo(tempFilePath);
                var newFileInfo = new FileInfo(newFilePath);
                using (var source = System.IO.File.OpenRead(tempFilePath))
                using (var excelPackage = new ExcelPackage(newFileInfo, templateFileInfo))
                {
                    // Open worksheet 1
                    ExcelWorksheet excelWorkSheet = excelPackage.Workbook.Worksheets[1];

                    excelWorkSheet.Column(1).Width = 18;
                    excelWorkSheet.Column(2).Width = 20;
                    excelWorkSheet.Column(3).Width = 20;

                    //Create data for exporting excel
                    for (int j = 0; j < lstOptl.Count; j++) //Loop for row.
                    {
                        //excelWorkSheet.Cells[j + startRow, 1].Value = lstOptl[j].ImagePath;
                        var cell = excelWorkSheet.Cells[j + startRow, 1];

                        
                        excelWorkSheet.Row(j + 2).Height = 55;

                        var image = "http://118.69.170.24:8005/OPS/ToolImages/" + lstOptl[j].ImagePath;

                        //Check image URL exists
                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(image);
                        request.Method = "HEAD";

                        bool exists;
                        try
                        {
                            request.GetResponse();
                            exists = true;
                        }
                        catch
                        {
                            exists = false;
                        }

                        if (exists == true)
                        {
                            WebClient wc = new WebClient();
                            byte[] bytes = wc.DownloadData(image);
                            MemoryStream ms = new MemoryStream(bytes);
                            Image img = Image.FromStream(ms);

                            var picture = excelWorkSheet.Drawings.AddPicture(lstOptl[j].ImagePath + j.ToString(), img);

                            picture.SetSize(115, 60);
                            picture.SetPosition(j + 1, 5, 0, 5);
                        }

                        else
                        {
                            excelWorkSheet.Cells[j + startRow, 1].Value = "";
                        }

                        excelWorkSheet.Cells[j + startRow, 2].Value = lstOptl[j].ItemCode;
                        excelWorkSheet.Cells[j + startRow, 3].Value = lstOptl[j].ItemName;

                        lastRow = j;

                    }
                    endRow = startRow + lastRow;
                    bottomRight = "D" + endRow; //End of row.

                    //Color border cell
                    using (ExcelRange range = excelWorkSheet.Cells["A" + startRow + ":C" + endRow])
                    {
                        range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Top.Color.SetColor(Color.Black);
                        range.Style.Border.Bottom.Color.SetColor(Color.Black);
                        range.Style.Border.Left.Color.SetColor(Color.Black);
                        range.Style.Border.Right.Color.SetColor(Color.Black);

                        range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    }

                    excelPackage.Save();
                    return excelPackage.Stream;
                }
            }
            catch 
            {
                throw;
            }

        }
        //END ADD: HA
    }
}