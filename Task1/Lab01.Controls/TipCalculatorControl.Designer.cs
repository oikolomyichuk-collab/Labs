namespace Lab01.Controls
{
    partial class TipCalculatorControl
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            txtAmount = new TextBox();
            numTip = new NumericUpDown();
            labelAmount = new Label();
            labelTip = new Label();
            labelResult = new Label();
            ((System.ComponentModel.ISupportInitialize)numTip).BeginInit();
            SuspendLayout();
            // 
            // txtAmount
            // 
            txtAmount.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtAmount.Location = new Point(164, 42);
            txtAmount.Name = "txtAmount";
            txtAmount.Size = new Size(189, 23);
            txtAmount.TabIndex = 0;
            txtAmount.TextChanged += txtAmount_TextChanged;
            // 
            // numTip
            // 
            numTip.Location = new Point(164, 97);
            numTip.Name = "numTip";
            numTip.Size = new Size(120, 23);
            numTip.TabIndex = 1;
            numTip.Tag = "";
            numTip.Value = new decimal(new int[] { 10, 0, 0, 0 });
            // 
            // labelAmount
            // 
            labelAmount.AutoSize = true;
            labelAmount.Location = new Point(94, 50);
            labelAmount.Name = "labelAmount";
            labelAmount.Size = new Size(42, 15);
            labelAmount.TabIndex = 2;
            labelAmount.Text = "Сума: ";
            // 
            // labelTip
            // 
            labelTip.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            labelTip.AutoSize = true;
            labelTip.Location = new Point(65, 105);
            labelTip.Name = "labelTip";
            labelTip.Size = new Size(71, 15);
            labelTip.TabIndex = 3;
            labelTip.Text = "Чайові (%): ";
            labelTip.Click += label2_Click;
            // 
            // labelResult
            // 
            labelResult.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            labelResult.AutoSize = true;
            labelResult.Location = new Point(145, 160);
            labelResult.Name = "labelResult";
            labelResult.Size = new Size(65, 15);
            labelResult.TabIndex = 4;
            labelResult.Text = "Підсумок: ";
            // 
            // TipCalculatorControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(labelResult);
            Controls.Add(labelTip);
            Controls.Add(labelAmount);
            Controls.Add(numTip);
            Controls.Add(txtAmount);
            Name = "TipCalculatorControl";
            Size = new Size(421, 219);
            ((System.ComponentModel.ISupportInitialize)numTip).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtAmount;
        private NumericUpDown numTip;
        private Label labelAmount;
        private Label labelTip;
        private Label labelResult;
    }
}
