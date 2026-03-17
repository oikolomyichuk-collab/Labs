using System.Drawing;
namespace Lab01.Controls
{
    [ToolboxBitmap(@"C:\Users\mysic\OneDrive\Рабочий стол\C#\Lab01.App\Lab01.Controls\TipCalculatorControl.ico")]
    public partial class TipCalculatorControl : UserControl
    {
        public TipCalculatorControl()
        {
            InitializeComponent();

            txtAmount.TextChanged += (s, e) => CalculateTotal();
            numTip.ValueChanged += (s, e) => CalculateTotal();
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
        private void CalculateTotal()
        {
            decimal amount;

            if (decimal.TryParse(txtAmount.Text, out amount))
            {
                decimal tip = amount * numTip.Value / 100;
                decimal total = amount + tip;

                labelResult.Text = total.ToString("Підсумок: 0.00");

                TotalChanged?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                labelResult.Text = "Введіть число!";
            }

        }

        private void txtAmount_TextChanged(object sender, EventArgs e)
        {

        }

        public decimal TipPercentage
        {
            get { return numTip.Value; }
            set { numTip.Value = value; }
        }

        public decimal Amount
        {
            get
            {
                decimal.TryParse(txtAmount.Text, out decimal value);
                return value;
            }
            set
            {
                txtAmount.Text = value.ToString();
            }
        }

        public decimal Total
        {
            get
            {
                decimal.TryParse(labelResult.Text, out decimal value);
                return value;
            }
        }

        public event EventHandler TotalChanged;
    }
}
