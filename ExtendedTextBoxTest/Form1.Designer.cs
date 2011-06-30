namespace ExtendedTextBoxTest
{
    partial class Form1
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.extendedTextBox1 = new Inclam.ExtendedTextBox();
            this.SuspendLayout();
            // 
            // extendedTextBox1
            // 
            this.extendedTextBox1.CustomRegex = "^.+$";
            this.extendedTextBox1.Location = new System.Drawing.Point(113, 46);
            this.extendedTextBox1.Name = "extendedTextBox1";
            this.extendedTextBox1.Size = new System.Drawing.Size(100, 20);
            this.extendedTextBox1.TabIndex = 0;
            this.extendedTextBox1.TypeOfData = Inclam.TYPE_DATA.DECIMAL;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 266);
            this.Controls.Add(this.extendedTextBox1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Inclam.ExtendedTextBox extendedTextBox1;

    }
}