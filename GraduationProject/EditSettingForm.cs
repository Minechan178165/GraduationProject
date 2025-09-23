using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GraduationProject
{
    public partial class EditSettingForm : Form
  
    {
        public EditSettingForm(string key, string value, string[] headers)
        {
            InitializeComponent();

            // 受け取った値を保持（表示は Form_Load で行う）
            KeyName = key;
            InitialValue = value;
            CsvHeaders = headers;

            this.Load += EditSettingForm_Load;
        }


        public string EditedValue { get;  set; }

         string KeyName;
         string InitialValue;
         string[] CsvHeaders;

       

         void EditSettingForm_Load(object sender, EventArgs e)
        {
            labelKey.Text = KeyName;

            LoadSourceGrid(CsvHeaders);
            //LoadOrderGrid(InitialValue, CsvHeaders);
        }


         void LoadSourceGrid(string[] headers)
        {
            var table = new DataTable();
            table.Columns.Add("項番");
            table.Columns.Add("項目名");
            table.Columns.Add("設定値"); // ← 追加！

            for (int i = 0; i < headers.Length; i++)
                table.Rows.Add(i, headers[i], ""); // ← 空欄で初期化

            dataGridViewSource.DataSource = table;
        }

        // void LoadSourceGrid(string[] headers)
        //{
        //    var table = new DataTable();
        //    table.Columns.Add("項番");
        //    table.Columns.Add("項目名");

        //    for (int i = 0; i < headers.Length; i++)
        //        table.Rows.Add(i, headers[i]);

        //    dataGridViewSource.DataSource = table;
        //}

        // void LoadOrderGrid(string value, string[] headers)
        //{
        //    var table = new DataTable();
        //    table.Columns.Add("項番");
        //    table.Columns.Add("項目名");

        //    var indices = value.Split(':')
        //        .Select(s => int.TryParse(s, out var i) ? i : -1)
        //        .Where(i => i >= 0 && i < headers.Length);

        //    foreach (var i in indices)
        //        table.Rows.Add(i, headers[i]);

        //    dataGridViewOrder.DataSource = table;
        //}

         void button_ok_excolumn_Click(object sender, EventArgs e)
        {
            var indices = dataGridViewSource.Rows.Cast<DataGridViewRow>()
    .Where(r => !r.IsNewRow)
    .Select(r => r.Cells[2].Value != null ? r.Cells[2].Value.ToString() : "")
    .ToArray();

            EditedValue = string.Join(":", indices);
            DialogResult = DialogResult.OK;
            Close();
        }

    }
}
