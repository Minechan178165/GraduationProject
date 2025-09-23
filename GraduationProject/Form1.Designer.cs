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
            dataGridView1 = new DataGridView();
            button_CSV = new Button();
            CSV_textBox = new TextBox();
            label2 = new Label();
            button_settingFile = new Button();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            SuspendLayout();
            // 
            // dataGridView1
            // 
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Location = new Point(126, 42);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.RowHeadersWidth = 62;
            dataGridView1.Size = new Size(916, 331);
            dataGridView1.TabIndex = 1;
            // 
            // button_CSV
            // 
            button_CSV.Location = new Point(841, 432);
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
            CSV_textBox.Location = new Point(448, 9);
            CSV_textBox.Name = "CSV_textBox";
            CSV_textBox.Size = new Size(648, 31);
            CSV_textBox.TabIndex = 9;
            CSV_textBox.Text = "C:\\Users\\kana-\\OneDrive\\プログラミング学習\\test.txt";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(126, 9);
            label2.Name = "label2";
            label2.Size = new Size(278, 25);
            label2.TabIndex = 8;
            label2.Text = "編集したいCSVをドラッグアンドドロップ";
            // 
            // button_settingFile
            // 
            button_settingFile.Location = new Point(588, 432);
            button_settingFile.Name = "button_settingFile";
            button_settingFile.Size = new Size(255, 58);
            button_settingFile.TabIndex = 7;
            button_settingFile.Text = "設定ファイルを編集する";
            button_settingFile.UseVisualStyleBackColor = true;
            button_settingFile.Click += button_settingFile_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1222, 524);
            Controls.Add(button_CSV);
            Controls.Add(CSV_textBox);
            Controls.Add(label2);
            Controls.Add(button_settingFile);
            Controls.Add(dataGridView1);
            Name = "Form1";
            Text = "Form1";
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
    }
}
