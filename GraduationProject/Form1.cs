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


        CsvEditor csvEditor = new CsvEditor();


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
            = Path.Combine(exeDir, Column.defoltSettingFileName);

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


        //　分割に関する変数
        int splitStandard; // 分割の基準

        int splitColumn;   // 分割する項番

        int splitCount;    // 分割する件数


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

            dataGridView1.CellToolTipTextNeeded += dataGridView1_CellToolTipTextNeeded;

            dataGridView1.VirtualMode = false;

            dataGridView1.ShowCellToolTips = true;

        }




        // テンプレ設定ファイルを表示するボタンのイベントハンドラ

        private void button_settingFile_Click(object sender, EventArgs e)
        {
            // DataGridViewにテンプレート設定を表示
            ShowSettings(settings);

            //　設定ファイルを編集するボタンを有効化
            Change_SettingFile_button.Enabled = true;

           
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
                orderStr = SettingParser.GetValues(settingsToSave, Column.columnExColumn)
                    .FirstOrDefault()?.Split(':');

                // バリデーション付きで項番を解析
                if (!IndexValidator.TryParseIndices
                    (orderStr ?? Array.Empty<string>(),
                    CsvHeaders.Length,
                    out var validOrder))                    
                    {
                        Log("項番解析失敗：妥当性チェックエラー");
                        return;
                    }

                // 項番の順序を整数配列に変換
                columnOrder = validOrder.ToArray();

                ColumnReorderDelegate reorder =
                    new ColumnReorderDelegate((row, order) => order.Select(i => row[i]).ToArray());





                // 連番が必要かどうかを取得
                bool renbanAdd = bool.TryParse(SettingParser.GetSingleValue
                    (settingsToSave, Column.columnRenbanNeed), out var result) && result;

                //　連番に付加する頭の文字を取得
                string renbanAddStr = SettingParser.GetSingleValue
                    (settingsToSave, Column.columnRenbanHead);

                if (renbanAdd)
                {
                    // 「連番」列をヘッダーに追加
                    CsvHeaders = CsvHeaders.Append("連番").ToArray();

                    int zeroPaddingDigits = int.Parse
                        (SettingParser.GetSingleValue(settingsToSave, Column.columnRenbanDigits));
                    int rowIndex = 0; // ← ヘッダー行は0番目としてカウント

                    reorder = (row, order) =>
                    {
                        var reordered = order.Select(i => row[i]).ToList();

                        if (rowIndex == 0)
                        {
                            // ヘッダー行には「連番」列名を追加
                            reordered.Add("連番");
                        }
                        else
                        {
                            // データ行には連番を追加
                            string paddedIndex = renbanAddStr+
                            rowIndex.ToString().PadLeft(zeroPaddingDigits, '0');

                            reordered.Add(paddedIndex);
                        }

                        rowIndex++;
                        return reordered.ToArray();
                    };



                    Log("連番追加設定：有効");
                }
                else
                {
                    //列順変更のデリゲートを定義
                   reorder = 
                        (row, order) => order.Select(i => row[i]).ToArray();


                    Log("連番追加設定：無効");
                }





                // CSVを編集して保存するための出力ファイル名を取得
                outputName = SettingParser.GetSingleValue
                    (settingsToSave, Column.columnOutFileName);
                string outputCsvPath = Path.Combine(csvDir, outputName);

                // 書き込みログ
                LogWrite(outputCsvPath);

                // CSVの項目を編集して保存
                CsvEditor.EditCsv(csvPath, outputCsvPath, columnOrder, encoding, reorder);


                // 分割処理（編集後のCSVを対象に）
                SpilitData(outputCsvPath);




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


        private void SpilitData(string editedCsvPath)
        {
            //　拡張子を除いた、出力するファイル名を取得
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(outputName);

            // 分割の基準を取得
            // 1: 件数で分割、2: 項番で分割
            // それ以外: 分割しない

            // 分割の基準が無効な場合のエラーチェック
            if (!int.TryParse(SettingParser.GetSingleValue(settingsToSave, Column.columnSplitStandard),
                out splitStandard))
            {
                MessageBox.Show("分割の基準が無効です。1（件数）または2（項番）を指定してください。",
                    "分割設定エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Log("分割設定エラー：分割の基準が無効");
                return;
            }

            // 分割の基準に応じて分割処理を実行

            // 件数で分割
            else if (splitStandard == 1)
            {
                if (!int.TryParse(SettingParser.GetSingleValue(settingsToSave, Column.columnSplitCount),
                    out splitCount))
                {
                    MessageBox.Show("分割件数が無効です。数値を入力してください。",
                        "分割件数エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Log("分割件数エラー：無効な数値");
                    return;
                }

                csvEditor.SplitCsvByCount(editedCsvPath, csvDir, splitCount,
                    encoding, fileNameWithoutExt);
                Log($"CSV分割完了：件数分割（{splitCount}件）");
            }

            // 項番で分割
            else if (splitStandard == 2)
            {
                if (!int.TryParse(SettingParser.GetSingleValue(settingsToSave, Column.columnSplitColumn),
                    out splitColumn))
                {
                    MessageBox.Show("分割項番が無効です。数値を入力してください。",
                        "分割項番エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Log("分割項番エラー：無効な数値");
                    return;
                }

                csvEditor.SplitCsvByColumnValue(editedCsvPath, csvDir, splitColumn,
                    encoding, fileNameWithoutExt);
                Log($"CSV分割完了：項番分割（項番 {splitColumn}）");
            }

            // 分割しない
            else
            {
                MessageBox.Show("分割の基準が無効です。1（件数）または2（項番）を指定してください。",
                    "分割設定エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Log($"CSV分割なし：無効な分割基準（{splitStandard}）");
            }
        }




        //　編集した設定ファイルを保存するボタンのイベントハンドラ
        private void Change_SettingFile_button_Click(object sender, EventArgs e)
        {
            CollectSettingsFromGrid(); // 編集内容を取得

            // 案件名の取得と初期化
            projectName = SettingParser.GetSingleValue(settingsToSave, Column.columnProjectName);
            if (string.IsNullOrWhiteSpace(projectName))
                projectName = Column.defoltOutputSettingFileName;

            csvPath = CSV_textBox.Text;
            csvDir = Path.GetDirectoryName(csvPath);


            // 文字コードを取得
            encoding = GetEncoding();

            // CSVのヘッダーを読み込む
            LoadCsvHeaders();

            // 項番の妥当性チェック
            var rawOrderValues = SettingParser.GetValues(settingsToSave, Column.columnExColumn)
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
                renameOutputSettingFileName = $"{Column.timeStamp}_{projectName}.txt";
                string outputSettingPath = Path.Combine(csvDir, renameOutputSettingFileName);

                // 設定ファイルを保存
                SaveSettings(settingsToSave, outputSettingPath);

                MessageBox.Show("設定ファイルを保存しました。", "保存完了", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Log($"編集設定ファイルを保存: {outputSettingPath}");

                // CSVファイルが指定されていれば、CSV編集ボタンを有効化
                if (!string.IsNullOrWhiteSpace(csvPath) && File.Exists(csvPath))
                    button_CSV.Enabled = true;
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
            encodingStr = SettingParser.GetValues(settingsToSave, Column.columnMojiCord)
                .FirstOrDefault()?.ToLower();

            encoding = encodingStr switch
            {
                "utf-16" => Encoding.Unicode,
                "shift_jis" => Encoding.GetEncoding("shift_jis"),
                "utf-8" => Encoding.UTF8,
                null or "" => Encoding.UTF8,
                _ => Encoding.UTF8
            };

            if (encodingStr != "utf-8" && encodingStr != "shift_jis" && encodingStr != "utf-16")
            {
                MessageBox.Show($"指定された文字コード「{encodingStr}」は未対応です。UTF-8で処理します。",
                    "文字コード警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Log($"未対応の文字コード指定：{encodingStr} → UTF-8で処理");
            }

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
            try
            {
                if (string.IsNullOrWhiteSpace(csvPath) || !File.Exists(csvPath))
                {
                    MessageBox.Show("CSVファイルが見つかりません。パスを確認してください。",
                        "ファイルエラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Log("CSVヘッダー読込失敗：ファイルが存在しない");
                    CsvHeaders = Array.Empty<string>();
                    return;
                }

                LogRead(csvPath);

                var firstLine = File.ReadLines(csvPath, encoding).FirstOrDefault();
                CsvHeaders = firstLine?.Split(',') ?? Array.Empty<string>();

                if (CsvHeaders.Length == 0)
                {
                    MessageBox.Show("CSVファイルにヘッダーが見つかりません。",
                        "ヘッダーエラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Log("CSVヘッダー読込失敗：空のヘッダー");
                }
                else
                {
                    Log("CSVヘッダー読込成功");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"CSVヘッダーの読込に失敗しました: {ex.Message}",
                    "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Log($"CSVヘッダー読込エラー: {ex.Message}");
                CsvHeaders = Array.Empty<string>();
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
            {
                target.Text = files[0];

                // CSVファイルのパスとディレクトリを取得
                csvPath = files[0];
                csvDir = Path.GetDirectoryName(csvPath);

                // CSVファイルがドラッグアンドドロップされたので、
                // テンプレ設定ファイルを設定ボタンを有効化
                button_settingFile.Enabled = true;

                // 初期化ログ（任意）
                Log($"CSVファイルが指定されました: {csvPath}");
            }
        }

        //　DataGridViewのセルにマウスが乗ったときのイベントハンドラ
        private void dataGridView1_CellToolTipTextNeeded(object sender, DataGridViewCellToolTipTextNeededEventArgs e)
        {
            toolTipSpilit.AutoPopDelay = 10000; // 10秒
            toolTipSpilit.InitialDelay = 200;   // 0.2秒で表示
            toolTipSpilit.ReshowDelay = 100;
            toolTipSpilit.ShowAlways = true;

            if (e.RowIndex >= 0 && e.ColumnIndex == 1)
            {
                string key = dataGridView1.Rows[e.RowIndex].Cells[0].Value?.ToString();
                string value = dataGridView1.Rows[e.RowIndex].Cells[1].Value?.ToString();

                if (string.IsNullOrEmpty(key))
                {
                    e.ToolTipText = $"値: {value}";
                    return;
                }

                e.ToolTipText = itemDescriptions.ContainsKey(key)
                    ? $"{itemDescriptions[key]}\n現在の値: {value}"
                    : $"項目: {key}\n値: {value}";
            }
        }

        readonly Dictionary<string, string> itemDescriptions = new()
{
    { "{分割の基準}", "1: 件数で分割\n2: 指定の項番で分割" },
    { "{文字コード}", "使用可能: utf-8, shift_jis, utf-16" },
    { "{連番の桁数}", "例: 6 → 000001, 000002, ..." },
    { "{連番の頭の文字}", "ファイル名の先頭に付加される識別子" },
    { "{抽出する項番}", "CSVの列番号を : 区切りで指定（例: 0:2:1）" },
    // 必要に応じて追加
};


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
            if (key == Column.columnExColumn)
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
            string today = DateTime.Now.ToString("yyyy-MM-dd");
            string logFileName = $"log_{today}.txt";
            string logPath = Path.Combine(csvDir, logFileName);
            string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";

            try
            {
                // ファイルが存在しない場合は新規作成（上書き）
                // 存在する場合は追記
                if (!File.Exists(logPath))
                {
                    File.WriteAllText(logPath, logEntry + Environment.NewLine, Encoding.UTF8);

                }
                else
                {
                    File.AppendAllText(logPath, logEntry + Environment.NewLine, Encoding.UTF8);
                }
            }
            catch (Exception ex)
            {
                // ログ出力に失敗した場合でもアプリが落ちないように
                // 可能なら、標準出力やデバッグ出力に記録
                try
                {
                    System.Diagnostics.Debug.WriteLine($"ログ出力失敗: {ex.Message}");
                }
                catch
                {
                    // Debug出力も失敗した場合は何もしない（完全に握りつぶす）
                }
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

        private void toolTipSpilit_Popup(object sender, PopupEventArgs e)
        {

        }
    }

}
