using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;

namespace Service.Helpers
{
    public static class ExcelTemplateHelper
    {
        public static byte[] GenerateStudentImportTemplate()
        {
            // Create workbook (XLSX format)
            IWorkbook workbook = new XSSFWorkbook();
            ISheet worksheet = workbook.CreateSheet("Students");

            // Create header row
            IRow headerRow = worksheet.CreateRow(0);
            
            // Set headers
            headerRow.CreateCell(0).SetCellValue("StudentCode");
            headerRow.CreateCell(1).SetCellValue("FullName");
            headerRow.CreateCell(2).SetCellValue("Email");

            // Create header style (bold)
            ICellStyle headerStyle = workbook.CreateCellStyle();
            IFont headerFont = workbook.CreateFont();
            headerFont.IsBold = true;
            headerStyle.SetFont(headerFont);

            // Apply header style
            for (int i = 0; i < 3; i++)
            {
                headerRow.GetCell(i).CellStyle = headerStyle;
            }

            // Add sample data
            IRow row1 = worksheet.CreateRow(1);
            row1.CreateCell(0).SetCellValue("SE183208");
            row1.CreateCell(1).SetCellValue("Nguyen Van A");
            row1.CreateCell(2).SetCellValue("anvn@example.com");

            IRow row2 = worksheet.CreateRow(2);
            row2.CreateCell(0).SetCellValue("SE183209");
            row2.CreateCell(1).SetCellValue("Le Thi B");
            row2.CreateCell(2).SetCellValue("blt@example.com");

            IRow row3 = worksheet.CreateRow(3);
            row3.CreateCell(0).SetCellValue("SE183210");
            row3.CreateCell(1).SetCellValue("Tran Van C");
            row3.CreateCell(2).SetCellValue("");

            // Auto-size columns
            for (int i = 0; i < 3; i++)
            {
                worksheet.AutoSizeColumn(i);
            }

            // Convert to byte array
            using var memoryStream = new MemoryStream();
            workbook.Write(memoryStream);
            workbook.Close();
            
            return memoryStream.ToArray();
        }
    }
}