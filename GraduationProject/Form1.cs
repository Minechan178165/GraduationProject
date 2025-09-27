using System.Data;
using System.Data.Common;
using System.Text;

namespace GraduationProject
{
    public partial class Form1 : Form
    {

        /// <summary>
        /// ★★　クラス内で使用する変数・定数
        /// ///////////////////////////////////////////////////////////////////////////////////
        /// </summary>      

        // 定数（readonly含む）

        // exeと同じ階層から3つ上のディレクトリを取得
        // exeと同じ階層から4つ上のディレクトリを取得
        static readonly string exeDir = Directory.GetParent(
            Directory.GetParent(
                Directory.GetParent(
                    Directory.GetParent(Application.StartupPath).FullName
                ).FullName
            ).FullName
        ).FullName;



        //　設定ファイルのテンプレートのフルパスを取得
        static readonly string templatePath
            = Path.Combine(exeDir, Column.defolt_settingFileName);

        // メンバ変数

        // CSVのヘッダーを格納する配列
        public string[] CsvHeaders { get; private set; }

        // 出力するファイル名
        string outputName;

        // CSVファイルのフルパス
        string csvPath;

        // CSVファイルのディレクトリ
        static string csvDir;


        // 項番の順序を取得（:区切り）
        static string[] orderStr;

        // 項番の順序を整数配列に変換
        static int[] columnOrder;

        // 文字コードを取得
        static string encodingStr;

        Encoding encoding;

        // 定数（readonly含む）

        //　出力する設定ファイル名
        string renameOutputSettingFileName;

        // 案件名
        string projectName;

        // 最初にDataGridViewに表示する、
        // 設定ファイルのテンプレートをDictionary形式で取得
        readonly Dictionary<string, List<string>> settings
           = SettingParser.ParseMultiple(templatePath);

        // DataGridViewにて編集した設定ファイルを保存するための変数
        readonly Dictionary<string, List<string>> settingsToSave = new();



        /// ///////////////////////////////////////////////////////////////////////////////////
        /// 



        public Form1()
        {
            InitializeComponent();


            CSV_textBox.DragEnter += TextBox_DragEnter;


            CSV_textBox.DragDrop += TextBox_DragDrop;


            dataGridView1.CellDoubleClick += DataGridView1_CellDoubleClick;
        }




        // 設定ファイルを編集するボタンのイベントハンドラ

        private void button_settingFile_Click(object sender, EventArgs e)
        {
            // DataGridViewにテンプレート設定を表示
            ShowSettings(settings);


            // DataGridViewの各行をループして設定を収集
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells[0].Value != null && row.Cells[1].Value != null)
                {
                    string key = row.Cells[0].Value.ToString();
                    string value = row.Cells[1].Value.ToString();

                    if (!settingsToSave.ContainsKey(key))
                        settingsToSave[key] = new List<string>();

                    settingsToSave[key].Add(value);
                }
            }



            //// 保存先の設定ファイルパス
            //// 案件名が見つからない場合は初期値を設定

            //projectName = SettingParser.GetSingleValue(settingsToSave, Column.column_projectName);

            //if (string.IsNullOrWhiteSpace(projectName))
            //{
            //    projectName = Column.defolt_output_settingFileName;
            //}



            //// CSVファイルのパスとディレクトリを取得
            //csvPath = CSV_textBox.Text;

            //csvDir = Path.GetDirectoryName(csvPath);



            //// ファイル名に使えない文字を除去（Windowsのファイル名制限対策）
            //foreach (char c in Path.GetInvalidFileNameChars())
            //{
            //    projectName = projectName.Replace(c.ToString(), "");
            //}

            //renameOutputSettingFileName =
            //$"{Column.timestamp}_{projectName}.txt";

            //// 日付を付加したファイル名にする
            //string outputSettingPath =
            //    Path.Combine(csvDir, renameOutputSettingFileName);

            //// 設定ファイルを保存
            //SaveSettings(settingsToSave, outputSettingPath);

        }

        // CSVファイルを編集するボタン
        // 新しい設定ファイルを読み込んでCSVを編集・保存するボタンのイベントハンドラ
        private void button_CSV_Click_1(object sender, EventArgs e)
        {
            try
            {
                // 文字コードを取得
                encoding = GetEncoding();

                // CSVのヘッダーを読み込む
                LoadCsvHeaders();

                //　CSVと同じフォルダにある設定ファイルを使用
                string editedSettingPath = Path.Combine(csvDir, renameOutputSettingFileName);

                //　読込ログ
                LogRead(editedSettingPath);

                // 設定ファイルを読み込む
                var settings = SettingParser.ParseMultiple(editedSettingPath);

                // 項番の文字列を取得
                orderStr = SettingParser.GetValues(settingsToSave, Column.column_exColumn)
                    .FirstOrDefault()?.Split(':');

                // バリデーション付きで項番を解析
                if (!IndexValidator.TryParseIndices
                    (orderStr ?? Array.Empty<string>(),
                    CsvHeaders.Length,
                    out var validOrder))
                    return; // エラーが出たら中断

                // 項番の順序を整数配列に変換
                columnOrder = validOrder.ToArray();



                // 列順変更のデリゲートを定義
                ColumnReorderDelegate reorder = (row, order) => order.Select(i => row[i]).ToArray();

                // CSVを編集して保存するための出力ファイル名を取得
                outputName = SettingParser.GetSingleValue
                    (settingsToSave, Column.column_outFileName);
                string outputCsvPath = Path.Combine(csvDir, outputName);

                // 書き込みログ
                LogWrite(outputCsvPath);

                // CSVを編集して保存
                CsvEditor.EditCsv(csvPath, outputCsvPath, columnOrder, encoding, reorder);

                MessageBox.Show("CSVの編集と保存が完了しました。",
                    "完了", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Log("CSV編集と保存完了");
            }

            // 例外処理
            catch (Exception ex)
            {
                MessageBox.Show($"CSV編集に失敗しました: {ex.Message}",
                    "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Log($"CSV編集エラー: {ex.Message}");
            }
        }


        //　編集した設定ファイルを保存するボタンのイベントハンドラ
        private void Change_SettingFile_button_Click(object sender, EventArgs e)
        {
            CollectSettingsFromGrid(); // 編集内容を取得

            // 案件名の取得と初期化
            projectName = SettingParser.GetSingleValue(settingsToSave, Column.column_projectName);
            if (string.IsNullOrWhiteSpace(projectName))
                projectName = Column.defolt_output_settingFileName;

            csvPath = CSV_textBox.Text;
            csvDir = Path.GetDirectoryName(csvPath);


            // 文字コードを取得
            encoding = GetEncoding();

            // CSVのヘッダーを読み込む
            LoadCsvHeaders();

            // 項番の妥当性チェック
            var rawOrderValues = SettingParser.GetValues(settingsToSave, Column.column_exColumn)
                .FirstOrDefault()?.Split(':') ?? Array.Empty<string>();

            if (!IndexValidator.TryParseIndices
                (rawOrderValues, CsvHeaders.Length, out var validOrder))
                return; // エラーが出たら保存処理を中断

            try
            {
                // ファイル名に使えない文字を除去（Windowsのファイル名制限対策）
                foreach (char c in Path.GetInvalidFileNameChars())
                    projectName = projectName.Replace(c.ToString(), "");

                // 日付を付加したファイル名にする
                renameOutputSettingFileName = $"{Column.timestamp}_{projectName}.txt";
                string outputSettingPath = Path.Combine(csvDir, renameOutputSettingFileName);

                // 設定ファイルを保存
                SaveSettings(settingsToSave, outputSettingPath);

                MessageBox.Show("設定ファイルを保存しました。", "保存完了", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Log($"編集設定ファイルを保存: {outputSettingPath}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"設定ファイルの保存に失敗しました: {ex.Message}", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Log($"編集設定ファイル保存エラー: {ex.Message}");
            }
        }


        //　DataGridViewの各行をループして設定を収集するメソッド
        //　これを使うと、編集後の内容をsettingsToSaveに反映できる
        void CollectSettingsFromGrid()
        {
            settingsToSave.Clear(); // ← 前回の内容をクリア

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells[0].Value != null && row.Cells[1].Value != null)
                {
                    string key = row.Cells[0].Value.ToString();
                    string value = row.Cells[1].Value.ToString();

                    if (!settingsToSave.ContainsKey(key))
                        settingsToSave[key] = new List<string>();

                    settingsToSave[key].Add(value);
                }
            }
        }



        //　文字コードを返す処理

        Encoding GetEncoding()
        {
            // 文字コードを取得
            encodingStr = SettingParser.GetValues
           (settingsToSave,
               Column.column_mojiCord).FirstOrDefault()?.ToLower();

            // 文字コードを設定
            encoding = encodingStr switch
            {
                "utf-16" => Encoding.Unicode,
                "shift_jis" => Encoding.GetEncoding("shift_jis"),
                "utf-8" => Encoding.UTF8,
                _ => Encoding.UTF8 // デフォルト
            };


            return encoding;
        }

        // DataGridViewに設定を表示
        void ShowSettings(Dictionary<string, List<string>> settings)
        {
            // DataTableを使用してDataGridViewにデータをバインド
            var table = new DataTable();
            table.Columns.Add("項目");
            table.Columns.Add("値");

            // settingsの内容をDataTableに追加
            foreach (var kv in settings)
            {
                string combinedValue = string.Join(", ", kv.Value);
                table.Rows.Add(kv.Key, combinedValue);
            }

            dataGridView1.DataSource = table;
        }

        // 設定ファイルを保存

        public static void SaveSettings
            (Dictionary<string, List<string>> settings, string path)
        {
            //　設定ファイルを書き込み保存
            try
            {
                using (var writer = new StreamWriter(path, false, Encoding.UTF8))
                {
                    foreach (var kv in settings)
                    {
                        foreach (var value in kv.Value)
                            writer.WriteLine($"{kv.Key}:{value}");
                    }
                }
                // 書き込みログ
                LogWrite(path);
            }

            // 例外処理
            catch (Exception ex)
            {
                MessageBox.Show($"設定ファイルの保存に失敗しました: {ex.Message}", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Log($"設定ファイル保存エラー: {ex.Message}");
            }
        }




        // CSVのヘッダーを読み込むメソッド
        void LoadCsvHeaders()
        {
            //　CSVを読み込んで、ヘッダーのみを取得
            try
            {
                // 読込ログ
                LogRead(csvPath);

                // CSVのヘッダーを読み込む
                var firstLine = File.ReadLines(csvPath, encoding).FirstOrDefault();
                CsvHeaders = firstLine?.Split(',') ?? new string[0];
                Log("CSVヘッダー読込成功");
            }

            // 例外処理
            catch (Exception ex)
            {
                MessageBox.Show($"CSVヘッダーの読込に失敗しました: {ex.Message}",
                    "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Log($"CSVヘッダー読込エラー: {ex.Message}");
            }
        }



        // ドラッグアンドドロップのイベントハンドラ
        void TextBox_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop)
                ? DragDropEffects.Copy
                : DragDropEffects.None;
        }

        void TextBox_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length > 0 && sender is TextBox target)
                target.Text = files[0];
        }






        // DataGridViewのセルがダブルクリックされたときのイベントハンドラ
        void DataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {

            encoding = GetEncoding(); // 文字コードを取得

            LoadCsvHeaders(); // ヘッダーを読み込む


            // 行と列のインデックスが有効か確認
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            // ダブルクリックされた行の「項目（キー）」を取得
            string key = dataGridView1.Rows[e.RowIndex].Cells[0].Value?.ToString();

            // 例：特定のキーに対して編集フォームを表示
            if (key == Column.column_exColumn)
            {
                var value = dataGridView1.Rows[e.RowIndex].Cells[1].Value?.ToString();

                var editForm = new EditSettingForm(key, value, CsvHeaders); // ← 別フォームに渡す



                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    // 編集後の値を反映
                    dataGridView1.Rows[e.RowIndex].Cells[1].Value = editForm.EditedValue;

                    // settingsToSave にも反映
                    if (!settingsToSave.ContainsKey(key))
                        settingsToSave[key] = new List<string>();
                    else
                        settingsToSave[key].Clear();

                    settingsToSave[key].Add(editForm.EditedValue);

                    // 再保存処理を呼び出す
                    string outputSettingPath = Path.Combine(csvDir, renameOutputSettingFileName);
                    SaveSettings(settingsToSave, outputSettingPath);
                }
            }


        }

        // ログ出力メソッド
        static void Log(string message)
        {
            string logPath = Path.Combine(csvDir, "log.txt");
            string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
            try
            {
                File.AppendAllText(logPath, logEntry + Environment.NewLine, Encoding.UTF8);
            }
            catch
            {
                // ログ出力に失敗してもアプリが落ちないように
            }
        }

        // 読込・書込のログを出力するメソッド
        static void LogRead(string filePath)
        {
            Log($"読込開始: {filePath}");
        }

        static void LogWrite(string filePath)
        {
            Log($"書き込み開始: {filePath}");
        }



        // 項番の妥当性をチェックするクラス
        public static class IndexValidator
        {

            //　項目番号の妥当性をチェックするメソッド
            public static bool TryParseIndices
                (IEnumerable<string> rawValues, int maxLength, out List<int> validIndices)
            {
                validIndices = new List<int>();
                var seen = new HashSet<int>();

                foreach (var val in rawValues)
                {
                    // 項番が整数であることを確認
                    // 範囲内であることを確認
                    // 重複していないことを確認
                    if (!int.TryParse(val, out int index) ||
                        index < 0 ||
                        index >= maxLength)
                    {
                        MessageBox.Show($"項番「{val}」は無効です。設定できません。",
                            "項番エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }

                    if (!seen.Add(index))
                    {
                        MessageBox.Show($"項番「{index}」が重複しています。重複は設定できません。",
                            "重複エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }

                    validIndices.Add(index);
                }

                return true;
            }
        }
    }

}
