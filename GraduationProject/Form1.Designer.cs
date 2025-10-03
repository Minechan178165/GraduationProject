namespace GraduationProject
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
         System.ComponentModel.IContainer components = null;

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
        void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            dataGridView1 = new DataGridView();
            button_CSV = new Button();
            CSV_textBox = new TextBox();
            label2 = new Label();
            button_settingFile = new Button();
            Change_SettingFile_button = new Button();
            toolTipSpilit = new ToolTip(components);
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            SuspendLayout();
            // 
            // dataGridView1
            // 
            dataGridView1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Location = new Point(126, 42);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.RowHeadersWidth = 62;
            dataGridView1.Size = new Size(916, 384);
            dataGridView1.TabIndex = 1;
            // 
            // button_CSV
            // 
            button_CSV.Anchor = AnchorStyles.Bottom;
            button_CSV.Enabled = false;
            button_CSV.Location = new Point(728, 454);
            button_CSV.Name = "button_CSV";
            button_CSV.Size = new Size(255, 58);
            button_CSV.TabIndex = 10;
            button_CSV.Text = "CSVを編集する";
            button_CSV.UseVisualStyleBackColor = true;
            button_CSV.Click += button_CSV_Click_1;
            // 
            // CSV_textBox
            // 
            CSV_textBox.AllowDrop = true;
            CSV_textBox.Anchor = AnchorStyles.Top;
            CSV_textBox.Location = new Point(444, 5);
            CSV_textBox.Name = "CSV_textBox";
            CSV_textBox.Size = new Size(594, 31);
            CSV_textBox.TabIndex = 9;
            // 
            // label2
            // 
            label2.Anchor = AnchorStyles.Top;
            label2.AutoSize = true;
            label2.Location = new Point(126, 9);
            label2.Name = "label2";
            label2.Size = new Size(278, 25);
            label2.TabIndex = 8;
            label2.Text = "編集したいCSVをドラッグアンドドロップ";
            // 
            // button_settingFile
            // 
            button_settingFile.Anchor = AnchorStyles.Bottom;
            button_settingFile.Enabled = false;
            button_settingFile.Location = new Point(165, 454);
            button_settingFile.Name = "button_settingFile";
            button_settingFile.Size = new Size(255, 58);
            button_settingFile.TabIndex = 7;
            button_settingFile.Text = "設定ファイルの初期値を設定する";
            button_settingFile.UseVisualStyleBackColor = true;
            button_settingFile.Click += button_settingFile_Click;
            // 
            // Change_SettingFile_button
            // 
            Change_SettingFile_button.Anchor = AnchorStyles.Bottom;
            Change_SettingFile_button.Enabled = false;
            Change_SettingFile_button.Location = new Point(448, 454);
            Change_SettingFile_button.Name = "Change_SettingFile_button";
            Change_SettingFile_button.Size = new Size(255, 58);
            Change_SettingFile_button.TabIndex = 11;
            Change_SettingFile_button.Text = "変更した設定ファイルを反映する";
            Change_SettingFile_button.UseVisualStyleBackColor = true;
            Change_SettingFile_button.Click += Change_SettingFile_button_Click;
            // 
            // toolTipSpilit
            // 
            toolTipSpilit.Popup += toolTipSpilit_Popup;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1222, 524);
            Controls.Add(Change_SettingFile_button);
            Controls.Add(button_CSV);
            Controls.Add(CSV_textBox);
            Controls.Add(label2);
            Controls.Add(button_settingFile);
            Controls.Add(dataGridView1);
            Name = "Form1";
            Text = "GraduationProject";
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        DataGridView dataGridView1;
         Button button_CSV;
         TextBox CSV_textBox;
         Label label2;
         Button button_settingFile;
        private Button Change_SettingFile_button;
        private ToolTip toolTipSpilit;
    }
}
