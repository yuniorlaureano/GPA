using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace GPA.Services.Report
{
    public interface IReportExcel
    {
        public XSSFWorkbook Workbook { get; }
        IReportExcel CreateWorkBook();
        ISheet CreateSheet(string sheetName);
        void CreateHeader(string[] headers);
        void CreateRowData<T>(IEnumerable<T> items, Action<IRow, T> rowBuilder);
    }

    public class ReportExcel : IReportExcel
    {
        public XSSFWorkbook Workbook { get; private set; }
        private ISheet sheet;
        public int NextRow { get; private set; } = 0;

        public IReportExcel CreateWorkBook()
        {
            NextRow = 0;
            Workbook = new XSSFWorkbook();
            return this;
        }

        public ISheet CreateSheet(string sheetName)
        {
            sheet = Workbook.CreateSheet(sheetName);
            return sheet;
        }

        public void CreateHeader(string[] headers)
        {
            IRow row = sheet.CreateRow(NextRow);
            for (int i = 0; i < headers.Length; i++)
            {
                row.CreateCell(i).SetCellValue(headers[i]);
            }
            NextRow++;
        }

        public void CreateRowData<T>(IEnumerable<T> items, Action<IRow, T> rowBuilder)
        {
            foreach (var item in items)
            {
                var row = sheet.CreateRow(NextRow);
                rowBuilder(row, item);
                NextRow++;
            }            
        }
    }
}
