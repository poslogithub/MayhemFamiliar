namespace MayhemFamiliar
{
    partial class MainForm
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
            tabControl1 = new TabControl();
            tabLog = new TabPage();
            textBoxLog = new TextBox();
            tabPage2 = new TabPage();
            tabControl1.SuspendLayout();
            tabLog.SuspendLayout();
            SuspendLayout();
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabLog);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Dock = DockStyle.Fill;
            tabControl1.Location = new Point(0, 0);
            tabControl1.Margin = new Padding(4, 5, 4, 5);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(1143, 750);
            tabControl1.TabIndex = 0;
            // 
            // tabLog
            // 
            tabLog.Controls.Add(textBoxLog);
            tabLog.Location = new Point(4, 34);
            tabLog.Margin = new Padding(4, 5, 4, 5);
            tabLog.Name = "tabLog";
            tabLog.Padding = new Padding(4, 5, 4, 5);
            tabLog.Size = new Size(1135, 712);
            tabLog.TabIndex = 0;
            tabLog.Text = "ログ";
            tabLog.UseVisualStyleBackColor = true;
            // 
            // textBoxLog
            // 
            textBoxLog.Dock = DockStyle.Fill;
            textBoxLog.Location = new Point(4, 5);
            textBoxLog.Margin = new Padding(4, 5, 4, 5);
            textBoxLog.Multiline = true;
            textBoxLog.Name = "textBoxLog";
            textBoxLog.ReadOnly = true;
            textBoxLog.ScrollBars = ScrollBars.Both;
            textBoxLog.Size = new Size(1127, 702);
            textBoxLog.TabIndex = 0;
            textBoxLog.WordWrap = false;
            // 
            // tabPage2
            // 
            tabPage2.Location = new Point(4, 34);
            tabPage2.Margin = new Padding(4, 5, 4, 5);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(4, 5, 4, 5);
            tabPage2.Size = new Size(1101, 672);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "tabPage2";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1143, 750);
            Controls.Add(tabControl1);
            Margin = new Padding(4, 5, 4, 5);
            Name = "MainForm";
            Text = "Form1";
            tabControl1.ResumeLayout(false);
            tabLog.ResumeLayout(false);
            tabLog.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private TabControl tabControl1;
        private TabPage tabLog;
        private TabPage tabPage2;
        private TextBox textBoxLog;
    }
}
