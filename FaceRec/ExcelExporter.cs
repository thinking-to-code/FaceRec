using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceRec
{
    public class ExcelExporter
    {
        public static void ExportToExcel(List<ExcelDataObject> dataList, string filePath)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Data");

                // Add headers
                worksheet.Cell(1, 1).Value = "ID";
                worksheet.Cell(1, 2).Value = "Image Path";
                worksheet.Cell(1, 3).Value = "Saved Face Path";
                worksheet.Cell(1, 4).Value = "Detected Face Count";
                worksheet.Cell(1, 5).Value = "Vendor";

                // Add data
                int row = 2;
                foreach (var data in dataList)
                {
                    worksheet.Cell(row, 1).Value = data.Id;
                    worksheet.Cell(row, 2).Value = data.ImagePath; 
                    worksheet.Cell(row, 3).Value = data.SavedFacePath;
                    worksheet.Cell(row, 4).Value = data.DetectedFaceCount.ToString();
                    worksheet.Cell(row, 5).Value = data.Vendor;
                    row++;
                }

                // Auto-adjust column width
                worksheet.Columns().AdjustToContents();

                // Save file
                workbook.SaveAs(filePath);
            }

            Console.WriteLine($"Excel file saved at {filePath}");
        }
    }
}
