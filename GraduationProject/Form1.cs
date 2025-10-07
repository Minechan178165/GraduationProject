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

            PrepareCsvEditingContext();

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
                Debug.LogRead(editedSettingPath);

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
                        Debug.Log("項番解析失敗：妥当性チェックエラー");
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



                    Debug.Log("連番追加設定：有効");
                }
                else
                {
                    //列順変更のデリゲートを定義
                   reorder = 
                        (row, order) => order.Select(i => row[i]).ToArray();


                    Debug.Log("連番追加設定：無効");
                }





                // CSVを編集して保存するための出力ファイル名を取得
                outputName = SettingParser.GetSingleValue
                    (settingsToSave, Column.columnOutFileName);
                string outputCsvPath = Path.Combine(csvDir, outputName);

                // 書き込みログ
                Debug.LogWrite(outputCsvPath);

                // CSVの項目を編集して保存
                CsvEditor.EditCsv(csvPath, outputCsvPath, columnOrder, encoding, reorder);


                // 分割処理（編集後のCSVを対象に）
                SpilitData(outputCsvPath);




                MessageBox.Show("CSVの編集と保存が完了しました。",
                    "完了", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Debug.Log("CSV編集と保存完了");
            }

            // 例外処理
            catch (Exception ex)
            {
                MessageBox.Show($"CSV編集に失敗しました: {ex.Message}",
                    "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Debug.Log($"CSV編集エラー: {ex.Message}");
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
                Debug.Log("分割設定エラー：分割の基準が無効");
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
                    Debug.Log("分割件数エラー：無効な数値");
                    return;
                }

                csvEditor.SplitCsvByCount(editedCsvPath, csvDir, splitCount,
                    encoding, fileNameWithoutExt);
                Debug.Log($"CSV分割完了：件数分割（{splitCount}件）");
            }

            // 項番で分割
            else if (splitStandard == 2)
            {
                if (!int.TryParse(SettingParser.GetSingleValue(settingsToSave, Column.columnSplitColumn),
                    out splitColumn))
                {
                    MessageBox.Show("分割項番が無効です。数値を入力してください。",
                        "分割項番エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Debug.Log("分割項番エラー：無効な数値");
                    return;
                }

                csvEditor.SplitCsvByColumnValue(editedCsvPath, csvDir, splitColumn,
                    encoding, fileNameWithoutExt);
                Debug.Log($"CSV分割完了：項番分割（項番 {splitColumn}）");
            }

            // 分割しない
            else
            {
                MessageBox.Show("分割の基準が無効です。1（件数）または2（項番）を指定してください。",
                    "分割設定エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Debug.Log($"CSV分割なし：無効な分割基準（{splitStandard}）");
            }
        }




        //　編集した設定ファイルを保存するボタンのイベントハンドラ
        private void Change_SettingFile_button_Click(object sender, EventArgs e)
        {

            PrepareCsvEditingContext();
            //CollectSettingsFromGrid(); // 編集内容を取得

            //// 案件名の取得と初期化
            //projectName = SettingParser.GetSingleValue(settingsToSave, Column.columnProjectName);
            //if (string.IsNullOrWhiteSpace(projectName))
            //    projectName = Column.defoltOutputSettingFileName;

            //csvPath = CSV_textBox.Text;
            //csvDir = Path.GetDirectoryName(csvPath);
            //Debug.csvDir = csvDir; // Debugクラス用に設定


            //// 文字コードを取得
            //encoding = GetEncoding();

            //// CSVのヘッダーを読み込む
            //LoadCsvHeaders();

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
                Debug.Log($"編集設定ファイルを保存: {outputSettingPath}");

                // CSVファイルが指定されていれば、CSV編集ボタンを有効化
                if (!string.IsNullOrWhiteSpace(csvPath) && File.Exists(csvPath))
                    button_CSV.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"設定ファイルの保存に失敗しました: {ex.Message}", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Debug.Log($"編集設定ファイル保存エラー: {ex.Message}");
            }
        }

        void PrepareCsvEditingContext()
        {
            // 編集内容を取得
            CollectSettingsFromGrid();

            // 案件名の取得と初期化
            projectName = SettingParser.GetSingleValue(settingsToSave, Column.columnProjectName);
            if (string.IsNullOrWhiteSpace(projectName))
                projectName = Column.defoltOutputSettingFileName;

            // CSVパスとディレクトリの取得
            csvPath = CSV_textBox.Text;
            csvDir = Path.GetDirectoryName(csvPath);
            Debug.csvDir = csvDir;

            // 文字コードの取得
            encoding = GetEncoding();

            // ヘッダーの読込
            LoadCsvHeaders();
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

            if (encodingStr != "utf-8" && encodingStr != "shift_jis" && encodingStr != "utf-16")
            {
                MessageBox.Show($"指定された文字コード「{encodingStr}」は未対応です。UTF-8で処理します。",
                    "文字コード警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Debug.Log($"未対応の文字コード指定：{encodingStr} → UTF-8で処理");
            }


            encoding = encodingStr switch
            {
                "utf-16" => Encoding.Unicode,
                "shift_jis" => Encoding.GetEncoding("shift_jis"),
                "utf-8" => Encoding.UTF8,
                null or "" => Encoding.UTF8,
                _ => Encoding.UTF8
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
                Debug.LogWrite(path);
            }

            // 例外処理
            catch (Exception ex)
            {
                MessageBox.Show($"設定ファイルの保存に失敗しました: {ex.Message}", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Debug.Log($"設定ファイル保存エラー: {ex.Message}");
            }
        }




        // CSVのヘッダーを読み込むメソッド
        //void LoadCsvHeaders()
        //{
        //    try
        //    {
        //        if (string.IsNullOrWhiteSpace(csvPath) || !File.Exists(csvPath))
        //        {
        //            MessageBox.Show("CSVファイルが見つかりません。パスを確認してください。",
        //                "ファイルエラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //            Debug.Log("CSVヘッダー読込失敗：ファイルが存在しない");
        //            CsvHeaders = Array.Empty<string>();
        //            return;
        //        }

        //        Debug.LogRead(csvPath);

        //        var firstLine = File.ReadLines(csvPath, encoding).FirstOrDefault();
        //        CsvHeaders = firstLine?.Split(',') ?? Array.Empty<string>();

        //        if (!CsvHeaders.Any())
        //            //if (CsvHeaders.Length == 0)
        //            {
        //            MessageBox.Show("CSVファイルにヘッダーが見つかりません。",
        //                "ヘッダーエラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //            Debug.Log("CSVヘッダー読込失敗：空のヘッダー");
        //        }
        //        else
        //        {
        //            Debug.Log("CSVヘッダー読込成功");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"CSVヘッダーの読込に失敗しました: {ex.Message}",
        //            "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //        Debug.Log($"CSVヘッダー読込エラー: {ex.Message}");
        //        CsvHeaders = Array.Empty<string>();
        //    }
        //}

        void LoadCsvHeaders()
        {
            try
            {
                if (!IsCsvFileValid(csvPath))
                {
                    MessageBox.Show("CSVファイルが見つかりません。パスを確認してください。",
                        "ファイルエラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Debug.Log("CSVヘッダー読込失敗：ファイルが存在しない");
                    CsvHeaders = Array.Empty<string>();
                    return;
                }

                Debug.LogRead(csvPath);

                CsvHeaders = ReadCsvHeaderLine(csvPath, encoding);

                if (!CsvHeaders.Any())
                {
                    MessageBox.Show("CSVファイルにヘッダーが見つかりません。",
                        "ヘッダーエラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Debug.Log("CSVヘッダー読込失敗：空のヘッダー");
                }
                else
                {
                    Debug.Log("CSVヘッダー読込成功");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"CSVヘッダーの読込に失敗しました: {ex.Message}",
                    "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Debug.Log($"CSVヘッダー読込エラー: {ex.Message}");
                CsvHeaders = Array.Empty<string>();
            }
        }

        bool IsCsvFileValid(string path)
        {
            return !string.IsNullOrWhiteSpace(path) && File.Exists(path);
        }

        string[] ReadCsvHeaderLine(string path, Encoding encoding)
        {
            return File.ReadLines(path, encoding).FirstOrDefault()?.Split(',') ?? Array.Empty<string>();
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

            var file=files?.FirstOrDefault();

            if(!string.IsNullOrWhiteSpace(file) && sender is TextBox target)
            {
                InitializeCsvPath(file, target);

                //target.Text = file;

                //// CSVファイルのパスとディレクトリを取得               
                //csvPath = file;
                //csvDir = Path.GetDirectoryName(csvPath);
                //Debug.csvDir = csvDir; // Debugクラス用に設定

                //// CSVファイルがドラッグアンドドロップされたので、
                //// テンプレ設定ファイルを設定ボタンを有効化
                //button_settingFile.Enabled = true;

                //// 初期化ログ（任意）
                //Debug.Log($"CSVファイルが指定されました: {csvPath}");
            }
        }


        void InitializeCsvPath(string file, TextBox target)
        {
            target.Text = file;
            csvPath = file;
            csvDir = Path.GetDirectoryName(csvPath);
            Debug.csvDir = csvDir;
            button_settingFile.Enabled = true;
            Debug.Log($"CSVファイルが指定されました: {csvPath}");
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


                e.ToolTipText = GetToolTipText(key, value);
                //if (string.IsNullOrEmpty(key))
                //{
                //    e.ToolTipText = $"値: {value}";
                //    return;
                //}

                //e.ToolTipText = itemDescriptions.ContainsKey(key)
                //    ? $"{itemDescriptions[key]}\n現在の値: {value}"
                //    : $"項目: {key}\n値: {value}";
            }
        }


        string GetToolTipText(string key, string value)
        {
            if (string.IsNullOrEmpty(key))
                return $"値: {value}";

            return itemDescriptions.ContainsKey(key)
                ? $"{itemDescriptions[key]}\n現在の値: {value}"
                : $"項目: {key}\n値: {value}";
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

                HandleEditColumnSetting(key, value, e.RowIndex);

                //var editForm = new EditSettingForm(key, value, CsvHeaders); // ← 別フォームに渡す




                //    if (editForm.ShowDialog() == DialogResult.OK)
                //    {
                //    // 編集後の値を反映
                //    dataGridView1.Rows[e.RowIndex].Cells[1].Value = editForm.EditedValue;

                //    // settingsToSave にも反映
                //    if (!settingsToSave.ContainsKey(key))
                //        settingsToSave[key] = new List<string>();
                //    else
                //        settingsToSave[key].Clear();

                //    settingsToSave[key].Add(editForm.EditedValue);

                //    // 再保存処理を呼び出す
                //    string outputSettingPath = Path.Combine(csvDir, renameOutputSettingFileName);
                //    SaveSettings(settingsToSave, outputSettingPath);
            }
            }


        


        void HandleEditColumnSetting(string key, string value, int rowIndex)
        {
            var editForm = new EditSettingForm(key, value, CsvHeaders);
            if (editForm.ShowDialog() == DialogResult.OK)
            {
                dataGridView1.Rows[rowIndex].Cells[1].Value = editForm.EditedValue;

                if (!settingsToSave.ContainsKey(key))
                    settingsToSave[key] = new List<string>();
                else
                    settingsToSave[key].Clear();

                settingsToSave[key].Add(editForm.EditedValue);

                //string outputSettingPath = Path.Combine(csvDir, renameOutputSettingFileName);
                //SaveSettings(settingsToSave, outputSettingPath);
            }
        }



        private void toolTipSpilit_Popup(object sender, PopupEventArgs e)
        {

        }
    }

}
