using Domain;
using ReportPluginBase;
using System.Runtime.InteropServices;
using Word = Microsoft.Office.Interop.Word;
namespace WordReportPlugin
{
    public class WordReport : IReportPlugin
    {
        public string Name => "Word Report";
        public string Description => "Експорт даних у документ Microsoft Word";

        public void GenerateReport()
        {
            dynamic wordApp = null;
            dynamic document = null;

            try
            {
                Type wordType = Type.GetTypeFromProgID("Word.Application");

                if (wordType == null)
                {
                    Console.WriteLine("Microsoft Word не знайдено.");
                    return;
                }

                wordApp = Activator.CreateInstance(wordType);
                document = wordApp.Documents.Add();

                using var storage = new Storage();
                storage.Database.EnsureCreated();

                dynamic paragraph = document.Paragraphs.Add();
                paragraph.Range.Text = "Звіт по виробництву деталей";
                paragraph.Range.Font.Bold = true;
                paragraph.Range.Font.Size = 16;
                paragraph.Range.InsertParagraphAfter();

                AddDetails(document, storage.Details.ToList());
                AddOperations(document, storage.Operations.ToList());
                AddProductions(document, storage.Productions.ToList());

                string path = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "WordReport.docx"
                );

                document.SaveAs2(path);
                Console.WriteLine("Word-звіт створено: " + path);
            }
            finally
            {
                if (document != null)
                {
                    document.Close();
                    Marshal.ReleaseComObject(document);
                }

                if (wordApp != null)
                {
                    wordApp.Quit();
                    Marshal.ReleaseComObject(wordApp);
                }

                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        private void AddDetails(dynamic document, List<Detail> details)
        {
            dynamic p = document.Paragraphs.Add();
            p.Range.Text = "Таблиця деталей";
            p.Range.InsertParagraphAfter();

            dynamic table = document.Tables.Add(p.Range, details.Count + 1, 5);

            table.Cell(1, 1).Range.Text = "Код";
            table.Cell(1, 2).Range.Text = "Децимальний номер";
            table.Cell(1, 3).Range.Text = "Назва";
            table.Cell(1, 4).Range.Text = "Марка сплаву";
            table.Cell(1, 5).Range.Text = "Маса";

            for (int i = 0; i < details.Count; i++)
            {
                table.Cell(i + 2, 1).Range.Text = details[i].DetailCode.ToString();
                table.Cell(i + 2, 2).Range.Text = details[i].DecimalNumber;
                table.Cell(i + 2, 3).Range.Text = details[i].DetailName;
                table.Cell(i + 2, 4).Range.Text = details[i].AlloyGrade;
                table.Cell(i + 2, 5).Range.Text = details[i].Mass.ToString();
            }

            table.Borders.Enable = 1;
        }

        private void AddOperations(dynamic document, List<Operation> operations)
        {
            dynamic p = document.Paragraphs.Add();
            p.Range.Text = "Таблиця операцій";
            p.Range.InsertParagraphAfter();

            dynamic table = document.Tables.Add(p.Range, operations.Count + 1, 4);

            table.Cell(1, 1).Range.Text = "Код";
            table.Cell(1, 2).Range.Text = "Номер цеху";
            table.Cell(1, 3).Range.Text = "Тривалість";
            table.Cell(1, 4).Range.Text = "Вартість";

            for (int i = 0; i < operations.Count; i++)
            {
                table.Cell(i + 2, 1).Range.Text = operations[i].OperationCode.ToString();
                table.Cell(i + 2, 2).Range.Text = operations[i].WorkshopNumber.ToString();
                table.Cell(i + 2, 3).Range.Text = operations[i].DurationHours.ToString();
                table.Cell(i + 2, 4).Range.Text = operations[i].Cost.ToString();
            }

            table.Borders.Enable = 1;
        }

        private void AddProductions(dynamic document, List<Production> productions)
        {
            dynamic p = document.Paragraphs.Add();
            p.Range.Text = "Таблиця виробництва";
            p.Range.InsertParagraphAfter();

            dynamic table = document.Tables.Add(p.Range, productions.Count + 1, 3);

            table.Cell(1, 1).Range.Text = "Код деталі";
            table.Cell(1, 2).Range.Text = "Номер операції";
            table.Cell(1, 3).Range.Text = "Код операції";

            for (int i = 0; i < productions.Count; i++)
            {
                table.Cell(i + 2, 1).Range.Text = productions[i].DetailCode.ToString();
                table.Cell(i + 2, 2).Range.Text = productions[i].OperationNumberInProcess.ToString();
                table.Cell(i + 2, 3).Range.Text = productions[i].OperationCode.ToString();
            }

            table.Borders.Enable = 1;
        }
    }
}