namespace GraduationProject
{
    partial class EditSettingForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
         System.ComponentModel.IContainer components = null;

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
         void InitializeComponent()
        {
            labelKey = new Label();
            dataGridViewSource = new DataGridView();
            label1 = new Label();
            button_ok_excolumn = new Button();
            ((System.ComponentModel.ISupportInitialize)dataGridViewSource).BeginInit();
            SuspendLayout();
            // 
            // labelKey
            // 
            labelKey.AutoSize = true;
            labelKey.Location = new Point(23, 19);
            labelKey.Name = "labelKey";
            labelKey.Size = new Size(59, 25);
            labelKey.TabIndex = 0;
            labelKey.Text = "label1";
            // 
            // dataGridViewSource
            // 
            dataGridViewSource.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewSource.Location = new Point(207, 89);
            dataGridViewSource.Name = "dataGridViewSource";
            dataGridViewSource.RowHeadersWidth = 62;
            dataGridViewSource.Size = new Size(589, 370);
            dataGridViewSource.TabIndex = 2;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(207, 19);
            label1.Name = "label1";
            label1.Size = new Size(194, 25);
            label1.TabIndex = 3;
            label1.Text = "元のCSVのファイルの項番";
            // 
            // button_ok_excolumn
            // 
            button_ok_excolumn.Location = new Point(481, 16);
            button_ok_excolumn.Name = "button_ok_excolumn";
            button_ok_excolumn.Size = new Size(112, 34);
            button_ok_excolumn.TabIndex = 4;
            button_ok_excolumn.Text = "button1";
            button_ok_excolumn.UseVisualStyleBackColor = true;
            button_ok_excolumn.Click += button_ok_excolumn_Click;
            // 
            // EditSettingForm
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1120, 488);
            Controls.Add(button_ok_excolumn);
            Controls.Add(label1);
            Controls.Add(dataGridViewSource);
            Controls.Add(labelKey);
            Name = "EditSettingForm";
            Text = "EditSettingForm";
            ((System.ComponentModel.ISupportInitialize)dataGridViewSource).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

         Label labelKey;
         DataGridView dataGridViewSource;
         Label label1;
         Button button_ok_excolumn;
        // Button button_OK_exColumn;
    }
}