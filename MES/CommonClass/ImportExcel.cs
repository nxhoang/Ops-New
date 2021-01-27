using System;
using System.Data;
using System.IO;
using System.Collections.Generic;
using System.Linq;

//Read EXCEL File from KPI Template
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace MES.CommonClass
{
    public class ImportExcel
    {
        public static bool ImportKPITemplateFile(string pDirectory, string pFileNameCollection)
        {
            try
            {
                if (String.IsNullOrEmpty(pFileNameCollection))
                {
                    return false;
                }

                var arrFileNameCollection = pFileNameCollection.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < arrFileNameCollection.Length; i++)
                {
                    var fCurrentName = arrFileNameCollection[i];
                    var path = string.Format("{0}\\{1}", pDirectory, fCurrentName);

                    if (System.IO.File.Exists(path))
                    {
                        var fileInfo = new System.IO.FileInfo(path);

                        switch (fileInfo.Extension)
                        {
                            case ".xlsx":
                                {
                                    ReadXMLExcel(pDirectory, fCurrentName);
                                }
                                break;
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                var Msg = ex.Message;
                return false;
            }
        }

        public static bool ReadXMLExcel(string pDirectory, string pFileName)
        {
            try
            {
                var path = string.Format("{0}\\{1}", pDirectory, pFileName);
                DataTable dt = new DataTable();

                if (File.Exists(path))
                {
                    //List<string> Headers = new List<string>();

                    // open the document read-only
                    SpreadsheetDocument document = SpreadsheetDocument.Open(path, false);

                    var wbPart = document.WorkbookPart;

                    Sheet theSheet = wbPart.Workbook.Descendants<Sheet>().Where(s => s.Name == "KPISetting").FirstOrDefault();

                    if (theSheet == null)
                    {
                        document.Close();
                        throw new ArgumentException("sheetName= \"KPISetting\" Not Found.");
                    }

                    WorksheetPart wsPart = (WorksheetPart)(wbPart.GetPartById(theSheet.Id));

                    Worksheet worksheet = (document.WorkbookPart.GetPartById(theSheet.Id.Value) as WorksheetPart).Worksheet;
                    IEnumerable<Row> rows = worksheet.GetFirstChild<SheetData>().Descendants<Row>();
                    int counter = 0;
                    foreach (Row row in rows)
                    {
                        counter = counter + 1;
                        //Read the first row as header
                        if (counter == 1)
                        {
                            var j = 1;
                            foreach (Cell cell in row.Descendants<Cell>())
                            {
                                var colunmName = true ? GetCellValue(document, cell) : "Field" + j++;
                                //Console.WriteLine(colunmName);
                                //Headers.Add(colunmName);
                                dt.Columns.Add(colunmName);
                            }
                        }
                        else
                        {
                            dt.Rows.Add();
                            //int i = 0;
                            foreach (Cell cell in row.Descendants<Cell>())
                            {
                                var temp_ = cell.CellReference.ToString().Substring(0, 1); 
                                switch (temp_)
                                {
                                    case "A":
                                        { dt.Rows[dt.Rows.Count - 1][0] = GetCellValue(document, cell); }
                                        break;
                                    case "B":
                                        { dt.Rows[dt.Rows.Count - 1][1] = GetCellValue(document, cell); }
                                        break;
                                    case "C":
                                        { dt.Rows[dt.Rows.Count - 1][2] = GetCellValue(document, cell); }
                                        break;
                                    case "D":
                                        { dt.Rows[dt.Rows.Count - 1][3] = GetCellValue(document, cell); }
                                        break;
                                    case "E":
                                        { dt.Rows[dt.Rows.Count - 1][4] = GetCellValue(document, cell); }
                                        break;
                                    case "F":
                                        { dt.Rows[dt.Rows.Count - 1][5] = GetCellValue(document, cell); }
                                        break;
                                    case "G":
                                        { dt.Rows[dt.Rows.Count - 1][6] = GetCellValue(document, cell); }
                                        break;
                                    case "H":
                                        { dt.Rows[dt.Rows.Count - 1][7] = GetCellValue(document, cell); }
                                        break;
                                    case "I":
                                        { dt.Rows[dt.Rows.Count - 1][8] = GetCellValue(document, cell); }
                                        break;
                                    case "J":
                                        { dt.Rows[dt.Rows.Count - 1][9] = GetCellValue(document, cell); }
                                        break;
                                    case "K":
                                        { dt.Rows[dt.Rows.Count - 1][10] = GetCellValue(document, cell); }
                                        break;
                                } 
                                //dt.Rows[dt.Rows.Count - 1][i] = GetCellValue(document, cell);
                                //i++;
                            }
                        }
                    }

                    document.Close();
                }

                if (dt != null)
                {
                    string System = "", Team = "", Corporation = "", Director = "", Buyer = "", Factory = "", Menu = "", Manager = "", LocalManager = "", Primary = "", InchargeStaff = "";
                    string AccumResult = "", Result = "";
                    foreach (DataRow dr in dt.Rows)
                    {
                        //Reset 
                        System = "";
                        Team = "";
                        Corporation = "";
                        Director = "";
                        Buyer = "";
                        Factory = "";
                        Menu = "";
                        Manager = "";
                        LocalManager = "";
                        Primary = "";
                        InchargeStaff = "";

                        //Current Row
                        System = dr["System"].ToString();
                        Team = dr["Team"].ToString();
                        Corporation = dr["Corporation"].ToString();
                        Director = dr["Director"].ToString();
                        Buyer = dr["Buyer"].ToString();
                        Factory = dr["Factory"].ToString();
                        Menu = dr["Menu"].ToString();
                        Manager = dr["Manager"].ToString();
                        LocalManager = dr["LocalManager"].ToString();
                        Primary = dr["Primary"].ToString();
                        InchargeStaff = dr["InchargeStaff"].ToString();

                        Result = "";
                        OPS_DAL.SystemBus.KPITeamBus.AddKPISetting("ByTemplate",System, Corporation, Team, Director, Buyer, Factory, Manager, LocalManager, Menu, Primary, InchargeStaff, out Result);
                        AccumResult = AccumResult + Result + "<BR/>";
                    }
                    dt.Dispose();
                }

                //Delete File
                //File.Delete(path);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static Sheets GetAllWorksheets(string fileName)
        {
            Sheets theSheets = null;

            using (SpreadsheetDocument document = SpreadsheetDocument.Open(fileName, false))
            {
                WorkbookPart wbPart = document.WorkbookPart;
                theSheets = wbPart.Workbook.Sheets;
            }
            return theSheets;
        }

        public static string GetCellValue(SpreadsheetDocument doc, Cell cell)
        {
            string value = cell.CellValue.InnerText;
            if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
            {
                return doc.WorkbookPart.SharedStringTablePart.SharedStringTable.ChildElements.GetItem(int.Parse(value)).InnerText;
            }
            return value;
        }
    }
}