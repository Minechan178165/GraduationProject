using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static GraduationProject.Form1;

namespace GraduationProject
{
    //　別フォームの処理をまとめたクラス
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


        public string EditedValue { get; set; }

        string KeyName;
        string InitialValue;
        string[] CsvHeaders;



        void EditSettingForm_Load(object sender, EventArgs e)
        {
            labelKey.Text = KeyName;

            LoadSourceGrid(CsvHeaders);
            //LoadOrderGrid(InitialValue, CsvHeaders);
        }

        //　データグリッドビューに項目名と項番を表示する
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

        //　設定ファイルに反映ボタンを押したときの処理
        void button_ok_excolumn_Click(object sender, EventArgs e)
        {
            // 入力された値を収集
            var rawValues = dataGridViewSource.Rows.Cast<DataGridViewRow>()
        .Where(r => !r.IsNewRow)
        .Select(r => r.Cells[2].Value?.ToString())
        .Where(val => !string.IsNullOrWhiteSpace(val));

            // 項番の妥当性をチェック
            if (!IndexValidator.TryParseIndices
                (rawValues, CsvHeaders.Length, out var validIndices))
                return;

            // 問題なければ結果を設定して閉じる
            EditedValue = string.Join(":", validIndices);
            DialogResult = DialogResult.OK;
            Close();



            //var indices = dataGridViewSource.Rows.Cast<DataGridViewRow>()
            //    .Where(r => !r.IsNewRow)
            //    .Select(r => r.Cells[2].Value?.ToString())
            //    .Where(val => !string.IsNullOrWhiteSpace(val))
            //    .ToArray();

            //var validIndices = new List<int>();
            //var seen = new HashSet<int>();

            //foreach (var indexStr in indices)
            //{
            //    //　項番が整数であることを確認
            //    //　範囲内であることを確認
            //    //　重複していないことを確認
            //    if (!int.TryParse(indexStr, out int index) ||
            //        index < 0 ||
            //        index >= CsvHeaders.Length)
            //    {
            //        MessageBox.Show($"項番「{indexStr}」は無効です。設定できません。",
            //            "項番エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //        return;
            //    }

            //    if (!seen.Add(index))
            //    {
            //        MessageBox.Show($"項番「{index}」が重複しています。重複は設定できません。", 
            //            "重複エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //        return;
            //    }

            //    validIndices.Add(index);
            //}

            //EditedValue = string.Join(":", validIndices);
            //DiaDebug.LogResult = DiaDebug.LogResult.OK;
            //Close();
        }

        //     void button_ok_excolumn_Click(object sender, EventArgs e)
        //    {
        //        var indices = dataGridViewSource.Rows.Cast<DataGridViewRow>()
        //.Where(r => !r.IsNewRow)
        //.Select(r => r.Cells[2].Value != null ? r.Cells[2].Value.ToString() : "")
        //.ToArray();

        //        EditedValue = string.Join(":", indices);
        //        DiaDebug.LogResult = DiaDebug.LogResult.OK;
        //        Close();
        //    }

    }





}
