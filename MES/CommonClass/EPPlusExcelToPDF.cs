using System;
using System.Collections.Generic;
using System.IO;
using OfficeOpenXml;
using PdfRpt.Core.Contracts;
using PdfRpt.FluentInterface;

namespace MES.CommonClass
{
    public class EPPlusExcelToPDF
    {

        public IPdfReportData CreateExcelToPdfReport(string filePath, string excelWorksheet , PageOrientation pPageOrientation , PdfPageSize pPdfPageSize)
        {
            try {
                var _path = filePath;
                var _location = (new FileInfo(filePath)).DirectoryName;
                var _fullName = (new FileInfo(filePath)).FullName;

                if (File.Exists(_path))
                    return new PdfReport().DocumentPreferences(doc =>
                    {
                        doc.RunDirection(PdfRunDirection.LeftToRight);
                        doc.Orientation(pPageOrientation);
                        doc.PageSize(pPdfPageSize);
                        doc.DocumentMetadata(new DocumentMetadata { Author = "Pungkook Saigon 2", Application = "MES", Keywords = "Pungkook", Subject = "QCO BOM", Title = "QCO BOM" });
                        doc.Compression(new CompressionSettings
                        {
                            EnableCompression = true,
                            EnableFullCompression = true
                        });
                    })
                        .DefaultFonts(fonts =>
                        {
                        //fonts.Path(TestUtils.GetVerdanaFontPath(),TestUtils.GetTahomaFontPath());
                        fonts.Size(10);
                            fonts.Color(System.Drawing.Color.Black);
                        })
                        .PagesFooter(footer =>
                        {
                            footer.DefaultFooter($"Print at {DateTime.Now.ToString("yyyy/MM/dd HH:mm")}");
                        })
                        //.PagesHeader(header =>
                        //{
                        //    header.CacheHeader(cache: true); // It's a default setting to improve the performance.
                        //header.DefaultHeader(defaultHeader =>
                        //    {
                        //        defaultHeader.RunDirection(PdfRunDirection.LeftToRight);
                        //    //defaultHeader.ImagePath(TestUtils.GetImagePath("01.png"));
                        //    defaultHeader.Message("");
                        //    });
                        //})
                        .MainTableTemplate(template =>
                        {
                            template.BasicTemplate(BasicTemplate.ClassicTemplate);
                        })
                        .MainTablePreferences(table =>
                        {
                            table.ColumnsWidthsType(TableColumnWidthType.FitToContent);
                            //table.MultipleColumnsPerPage(new MultipleColumnsPerPage
                            //{
                            //    ColumnsGap = 7,
                            //    ColumnsPerPage = 1,
                            //    ColumnsWidth = 1024,
                            //    IsRightToLeft = false,
                            //    TopMargin = 7
                            //});
                        })
                        .MainTableDataSource(dataSource =>
                        {
                            dataSource.CustomDataSource(() => new ExcelDataReaderDataSource(filePath, excelWorksheet));
                        })
                        .MainTableColumns(columns =>
                        { 
                            //Add 1st Column #
                            //columns.AddColumn(column =>
                            //{
                            //    column.PropertyName("rowNo");
                            //    column.IsRowNumber(true);
                            //    column.CellsHorizontalAlignment(HorizontalAlignment.Left);
                            //    column.IsVisible(true);
                            //    column.Order(0);
                            //    column.Width(1);
                            //    column.HeaderCell("#");
                            //});

                            var order = 1;
                            foreach (var columnInfo in ExcelUtils.GetColumns(filePath, excelWorksheet))
                            {
                                columns.AddColumn(column =>
                                {
                                    column.PropertyName(columnInfo);
                                    column.CellsHorizontalAlignment(HorizontalAlignment.Left);
                                    column.IsVisible(true);
                                    column.Order(order++);
                                    column.Width(1);
                                    column.HeaderCell(columnInfo);
                                });
                            }
                        })
                        .MainTableEvents(events =>
                        {
                            events.DataSourceIsEmpty(message: "There is no data available to display.");
                        })
                        .Generate(data => data.AsPdfFile(System.IO.Path.Combine(_location, $"{ _fullName.Split('.')[0] }.pdf")));
                else
                    return null;
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                return null;
            }
        }

        public IPdfReportData CreateExcelToPdfReport(ExcelPackage package, string excelWorksheet)
        {
            try
            {
                if (package != null)
                {
                    System.IO.MemoryStream stream = new System.IO.MemoryStream();
                    //package.SaveAs(stream);

                    return new PdfReport().DocumentPreferences(doc =>
                    {
                        doc.RunDirection(PdfRunDirection.LeftToRight);
                        doc.Orientation(PageOrientation.Portrait);
                        doc.PageSize(PdfPageSize.A4);
                        doc.DocumentMetadata(new DocumentMetadata { Author = "Vahid", Application = "PdfRpt", Keywords = "Test", Subject = "Test Rpt", Title = "Test" });
                        doc.Compression(new CompressionSettings
                        {
                            EnableCompression = true,
                            EnableFullCompression = true
                        });
                    })
                        .DefaultFonts(fonts =>
                        {
                            fonts.Size(9);
                            fonts.Color(System.Drawing.Color.Black);
                        })
                        .PagesFooter(footer =>
                        {
                            footer.DefaultFooter(DateTime.Now.ToString("MM/dd/yyyy"));
                        })
                        .PagesHeader(header =>
                        {
                            header.CacheHeader(cache: true); // It's a default setting to improve the performance.
                        header.DefaultHeader(defaultHeader =>
                                {
                                    defaultHeader.RunDirection(PdfRunDirection.LeftToRight);
                                    defaultHeader.Message("Excel Package To Pdf Report");
                                });
                        })
                        .MainTableTemplate(template =>
                        {
                            template.BasicTemplate(BasicTemplate.ClassicTemplate);
                        })
                        .MainTablePreferences(table =>
                        {
                            table.ColumnsWidthsType(TableColumnWidthType.Relative);
                            table.MultipleColumnsPerPage(new MultipleColumnsPerPage
                            {
                                ColumnsGap = 7,
                                ColumnsPerPage = 3,
                                ColumnsWidth = 170,
                                IsRightToLeft = false,
                                TopMargin = 7
                            });
                        })
                        .MainTableDataSource(dataSource =>
                        {
                            dataSource.CustomDataSource(() => new ExcelDataReaderDataSourceByPackage(package, excelWorksheet));
                        })
                        .MainTableColumns(columns =>
                        {
                            //Add 1st Column #
                            //columns.AddColumn(column =>
                            //{
                            //    column.PropertyName("rowNo");
                            //    column.IsRowNumber(true);
                            //    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                            //    column.IsVisible(true);
                            //    column.Order(0);
                            //    column.Width(1);
                            //    column.HeaderCell("#");
                            //}); 
                            var order = 1;
                            foreach (var columnInfo in ExcelUtils.GetColumns(package, excelWorksheet))
                            {
                                columns.AddColumn(column =>
                                {
                                    column.PropertyName(columnInfo);
                                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                                    column.IsVisible(true);
                                    column.Order(order++);
                                    column.Width(1);
                                    column.HeaderCell(columnInfo);
                                });
                            }
                        })
                        .MainTableEvents(events =>
                        {
                            events.DataSourceIsEmpty(message: "There is no data available to display.");
                        })
                        .Generate(data => data.AsPdfStream(stream, false));
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                var Msg = ex.Message;
                return null; 
            }
        }

    }

    public class ExcelDataReaderDataSourceByPackage : IDataSource
    { 
        private readonly string _worksheet;
        private ExcelPackage _excelPackage;
        public ExcelDataReaderDataSourceByPackage(ExcelPackage package, string worksheet)
        {
            _excelPackage = package;
            _worksheet = worksheet;
        }

        public IEnumerable<IList<CellData>> Rows()
        {
            var worksheet = _excelPackage.Workbook.Worksheets[_worksheet];
            var startCell = worksheet.Dimension.Start;
            var endCell = worksheet.Dimension.End;

            for (var row = startCell.Row + 1; row < endCell.Row + 1; row++)
            {
                var i = 0;
                var result = new List<CellData>();
                for (var col = startCell.Column; col <= endCell.Column; col++)
                {
                    var pdfCellData = new CellData
                    {
                        PropertyName = worksheet.Cells[1, col].Value.ToString(),
                        PropertyValue = worksheet.Cells[row, col].Value,
                        PropertyIndex = i++
                    };
                    result.Add(pdfCellData);
                }
                yield return result;
            } 
        }
    }

    public class ExcelDataReaderDataSource : IDataSource
    {
        private readonly string _filePath;
        private readonly string _worksheet;
        private readonly ExcelPackage _excelPackage;

        public ExcelDataReaderDataSource(string filePath, string worksheet)
        {
            _filePath = filePath;
            _worksheet = worksheet;
        }

        public ExcelDataReaderDataSource(ExcelPackage package, string worksheet)
        {
            _excelPackage = package;
            _worksheet = worksheet;
        }

        public IEnumerable<IList<CellData>> Rows()
        {
            var fileInfo = new FileInfo(_filePath);
            if (!fileInfo.Exists)
            {
                throw new FileNotFoundException($"{_filePath} file not found.");
            }

            using (var package = new ExcelPackage(fileInfo))
            {
                var worksheet = package.Workbook.Worksheets[_worksheet];
                var startCell = worksheet.Dimension.Start;
                var endCell = worksheet.Dimension.End;

                for (var row = startCell.Row + 1; row < endCell.Row + 1; row++)
                {
                    var i = 0;
                    var result = new List<CellData>();
                    for (var col = startCell.Column; col <= endCell.Column; col++)
                    {
                        var pdfCellData = new CellData
                        {
                            PropertyName = worksheet.Cells[1, col].Value.ToString(),
                            PropertyValue = worksheet.Cells[row, col].Value,
                            PropertyIndex = i++
                        };
                        result.Add(pdfCellData);
                    }
                    yield return result;
                }
            }

        }
         
        public IEnumerable<IList<CellData>> RowsByExcelPackage()
        {
            using (var package = _excelPackage)
            {
                var worksheet = package.Workbook.Worksheets[_worksheet];
                var startCell = worksheet.Dimension.Start;
                var endCell = worksheet.Dimension.End;

                for (var row = startCell.Row + 1; row < endCell.Row + 1; row++)
                {
                    var i = 0;
                    var result = new List<CellData>();
                    for (var col = startCell.Column; col <= endCell.Column; col++)
                    {
                        var pdfCellData = new CellData
                        {
                            PropertyName = worksheet.Cells[1, col].Value.ToString(),
                            PropertyValue = worksheet.Cells[row, col].Value,
                            PropertyIndex = i++
                        };
                        result.Add(pdfCellData);
                    }
                    yield return result;
                }
            }
        }
    }

    public static class ExcelUtils
    {
        public static IList<string> GetColumns(string filePath, string excelWorksheet)
        {
            var fileInfo = new FileInfo(filePath);
            if (!fileInfo.Exists)
            {
                throw new FileNotFoundException($"{filePath} file not found.");
            }

            var columns = new List<string>();
            using (var package = new ExcelPackage(fileInfo))
            {
                var worksheet = package.Workbook.Worksheets[excelWorksheet];
                var startCell = worksheet.Dimension.Start;
                var endCell = worksheet.Dimension.End;

                for (int col = startCell.Column; col <= endCell.Column; col++)
                {
                    var colHeader = worksheet.Cells[1, col].Value.ToString();
                    columns.Add(colHeader);
                }
            }
            return columns;
        }

        public static IList<string> GetColumns(ExcelPackage package, string excelWorksheet)
        {
            var columns = new List<string>();

            var worksheet = package.Workbook.Worksheets[excelWorksheet];
            var startCell = worksheet.Dimension.Start;
            var endCell = worksheet.Dimension.End;

            for (int col = startCell.Column; col <= endCell.Column; col++)
            {
                var colHeader = worksheet.Cells[1, col].Value.ToString();
                columns.Add(colHeader);
            }

            return columns;
        }
    }

}