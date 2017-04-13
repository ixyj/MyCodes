
namespace ExcelHelper
{
    using Microsoft.Office.Interop.Excel;
    using System;


    /*******
     * Need Excel.dll, which could be generated from command "TlbImp excel.exe /out:excel.dll"
     * Unsupport merged cells (all are null except for the 1st one)   
     * *******/
    class ExcelHelper
    {
        public static object[,] ReadExcel(string fileName, int sheet = 1, string startIndex = "A1", string endIndex = null)
        {
            var excelApp = new Application
            {
                Visible = false,
                UserControl = true
            };

            var missing = System.Reflection.Missing.Value;
            var workbook = excelApp.Application.Workbooks.Open(fileName, missing, true, missing, missing, missing, missing, missing, missing, true, missing, missing, missing, missing, missing);
            var worksheet = (Worksheet)workbook.Worksheets.Item[sheet];
            var range = worksheet.Cells.Range[startIndex, endIndex ?? "A" + worksheet.UsedRange.Cells.Rows.Count];
            var result = (object[,])range.Value;
            excelApp.Quit();

            return result;
        }

        static void Main()
        {
            var result = ReadExcel(@"sample.xlsx", 1, "a2", "j108");

            for (var row = result.GetLowerBound(0); row <= result.GetUpperBound(0); ++row)
            {
                for (var col = result.GetLowerBound(1); col <= result.GetUpperBound(1); ++col)
                {
                    Console.WriteLine(result[row, col]);
                }
            }
        }
    }
}
