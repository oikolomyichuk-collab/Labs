namespace Lab01.App
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            tipCalculatorControl1 = new Lab01.Controls.TipCalculatorControl();
            SuspendLayout();
            // 
            // tipCalculatorControl1
            // 
            tipCalculatorControl1.Amount = new decimal(new int[] { 0, 0, 0, 0 });
            tipCalculatorControl1.Location = new Point(183, 102);
            tipCalculatorControl1.Name = "tipCalculatorControl1";
            tipCalculatorControl1.Size = new Size(436, 230);
            tipCalculatorControl1.TabIndex = 0;
            tipCalculatorControl1.TipPercentage = new decimal(new int[] { 10, 0, 0, 0 });
            tipCalculatorControl1.Load += tipCalculatorControl1_Load;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(tipCalculatorControl1);
            Name = "Form1";
            Text = "ы";
            Load += Form1_Load;
            ResumeLayout(false);
        }

        #endregion

        private Controls.TipCalculatorControl tipCalculatorControl1;
    }
}
