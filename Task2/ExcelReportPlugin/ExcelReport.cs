using Domain;
using ReportPluginBase;
using System.Runtime.InteropServices;

namespace ExcelReportPlugin
{
    public class ExcelReport : IReportPlugin
    {
        public string Name => "Excel Report";

        public string Description => "Експорт даних у таблицю Microsoft Excel";

        public void GenerateReport()
        {
            dynamic excelApp = null;
            dynamic workbook = null;
            dynamic sheet = null;

            try
            {
                Type excelType = Type.GetTypeFromProgID("Excel.Application");

                if (excelType == null)
                {
                    Console.WriteLine("Microsoft Excel не знайдено.");
                    return;
                }

                excelApp = Activator.CreateInstance(excelType);
                workbook = excelApp.Workbooks.Add();
                sheet = workbook.Worksheets[1];

                using var storage = new Storage();
                storage.Database.EnsureCreated();

                int row = 1;

                sheet.Cells[row, 1] = "Звіт по виробництву деталей";
                row += 2;

                row = AddDetails(sheet, row, storage.Details.ToList());
                row += 2;

                row = AddOperations(sheet, row, storage.Operations.ToList());
                row += 2;

                AddProductions(sheet, row, storage.Productions.ToList());

                sheet.Columns.AutoFit();

                string path = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "ExcelReport.xlsx"
                );

                workbook.SaveAs(path);

                Console.WriteLine("Excel-звіт створено: " + path);
            }
            finally
            {
                if (workbook != null)
                {
                    workbook.Close(false);
                    Marshal.ReleaseComObject(workbook);
                }

                if (sheet != null)
                {
                    Marshal.ReleaseComObject(sheet);
                }

                if (excelApp != null)
                {
                    excelApp.Quit();
                    Marshal.ReleaseComObject(excelApp);
                }

                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        private int AddDetails(dynamic sheet, int row, List<Detail> details)
        {
            sheet.Cells[row, 1] = "Деталі";
            row++;

            sheet.Cells[row, 1] = "Код";
            sheet.Cells[row, 2] = "Децимальний номер";
            sheet.Cells[row, 3] = "Назва";
            sheet.Cells[row, 4] = "Марка сплаву";
            sheet.Cells[row, 5] = "Маса";
            row++;

            foreach (var d in details)
            {
                sheet.Cells[row, 1] = d.DetailCode;
                sheet.Cells[row, 2] = d.DecimalNumber;
                sheet.Cells[row, 3] = d.DetailName;
                sheet.Cells[row, 4] = d.AlloyGrade;
                sheet.Cells[row, 5] = d.Mass;
                row++;
            }

            return row;
        }

        private int AddOperations(dynamic sheet, int row, List<Operation> operations)
        {
            sheet.Cells[row, 1] = "Операції";
            row++;

            sheet.Cells[row, 1] = "Код";
            sheet.Cells[row, 2] = "Номер цеху";
            sheet.Cells[row, 3] = "Тривалість, год";
            sheet.Cells[row, 4] = "Вартість, грн";
            row++;

            foreach (var o in operations)
            {
                sheet.Cells[row, 1] = o.OperationCode;
                sheet.Cells[row, 2] = o.WorkshopNumber;
                sheet.Cells[row, 3] = o.DurationHours;
                sheet.Cells[row, 4] = o.Cost;
                row++;
            }

            return row;
        }

        private int AddProductions(dynamic sheet, int row, List<Production> productions)
        {
            sheet.Cells[row, 1] = "Виробництво";
            row++;

            sheet.Cells[row, 1] = "Код деталі";
            sheet.Cells[row, 2] = "Номер операції";
            sheet.Cells[row, 3] = "Код операції";
            row++;

            foreach (var p in productions)
            {
                sheet.Cells[row, 1] = p.DetailCode;
                sheet.Cells[row, 2] = p.OperationNumberInProcess;
                sheet.Cells[row, 3] = p.OperationCode;
                row++;
            }

            return row;
        }
    }
}