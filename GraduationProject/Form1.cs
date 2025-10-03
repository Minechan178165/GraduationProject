using System.Data;
using System.Data.Common;
using System.Text;

namespace GraduationProject
{
    public partial class Form1 : Form
    {

        /// <summary>
        /// �����@�N���X���Ŏg�p����ϐ��E�萔
        /// ///////////////////////////////////////////////////////////////////////////////////
        /// </summary>      


        CsvEditor csvEditor = new CsvEditor();


        // �萔�ireadonly�܂ށj

        // exe�Ɠ����K�w����3��̃f�B���N�g�����擾
        // exe�Ɠ����K�w����4��̃f�B���N�g�����擾
        static readonly string exeDir = Directory.GetParent(
            Directory.GetParent(
                Directory.GetParent(
                    Directory.GetParent(Application.StartupPath).FullName
                ).FullName
            ).FullName
        ).FullName;



        //�@�ݒ�t�@�C���̃e���v���[�g�̃t���p�X���擾
        static readonly string templatePath
            = Path.Combine(exeDir, Column.defoltSettingFileName);

        // �����o�ϐ�

        // CSV�̃w�b�_�[���i�[����z��
        public string[] CsvHeaders { get; private set; }

        // �o�͂���t�@�C����
        string outputName;

        // CSV�t�@�C���̃t���p�X
        string csvPath;

        // CSV�t�@�C���̃f�B���N�g��
        static string csvDir;


        // ���Ԃ̏������擾�i:��؂�j
        static string[] orderStr;

        // ���Ԃ̏����𐮐��z��ɕϊ�
        static int[] columnOrder;

        // �����R�[�h���擾
        static string encodingStr;

        Encoding encoding;


        //�@�����Ɋւ���ϐ�
        int splitStandard; // �����̊

        int splitColumn;   // �������鍀��

        int splitCount;    // �������錏��


        // �萔�ireadonly�܂ށj

        //�@�o�͂���ݒ�t�@�C����
        string renameOutputSettingFileName;

        // �Č���
        string projectName;

        // �ŏ���DataGridView�ɕ\������A
        // �ݒ�t�@�C���̃e���v���[�g��Dictionary�`���Ŏ擾
        readonly Dictionary<string, List<string>> settings
           = SettingParser.ParseMultiple(templatePath);

        // DataGridView�ɂĕҏW�����ݒ�t�@�C����ۑ����邽�߂̕ϐ�
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




        // �e���v���ݒ�t�@�C����\������{�^���̃C�x���g�n���h��

        private void button_settingFile_Click(object sender, EventArgs e)
        {
            // DataGridView�Ƀe���v���[�g�ݒ��\��
            ShowSettings(settings);

            //�@�ݒ�t�@�C����ҏW����{�^����L����
            Change_SettingFile_button.Enabled = true;

           
        }

        // CSV�t�@�C����ҏW����{�^��
        // �V�����ݒ�t�@�C����ǂݍ����CSV��ҏW�E�ۑ�����{�^���̃C�x���g�n���h��
        private void button_CSV_Click_1(object sender, EventArgs e)
        {
            try
            {
                // �����R�[�h���擾
                encoding = GetEncoding();

                // CSV�̃w�b�_�[��ǂݍ���
                LoadCsvHeaders();

                
               


                //�@CSV�Ɠ����t�H���_�ɂ���ݒ�t�@�C�����g�p
                string editedSettingPath = Path.Combine(csvDir, renameOutputSettingFileName);

                //�@�Ǎ����O
                LogRead(editedSettingPath);

                // �ݒ�t�@�C����ǂݍ���
                var settings = SettingParser.ParseMultiple(editedSettingPath);




                // ���Ԃ̕�������擾
                orderStr = SettingParser.GetValues(settingsToSave, Column.columnExColumn)
                    .FirstOrDefault()?.Split(':');

                // �o���f�[�V�����t���ō��Ԃ����
                if (!IndexValidator.TryParseIndices
                    (orderStr ?? Array.Empty<string>(),
                    CsvHeaders.Length,
                    out var validOrder))                    
                    {
                        Log("���ԉ�͎��s�F�Ó����`�F�b�N�G���[");
                        return;
                    }

                // ���Ԃ̏����𐮐��z��ɕϊ�
                columnOrder = validOrder.ToArray();

                ColumnReorderDelegate reorder =
                    new ColumnReorderDelegate((row, order) => order.Select(i => row[i]).ToArray());





                // �A�Ԃ��K�v���ǂ������擾
                bool renbanAdd = bool.TryParse(SettingParser.GetSingleValue
                    (settingsToSave, Column.columnRenbanNeed), out var result) && result;

                //�@�A�Ԃɕt�����铪�̕������擾
                string renbanAddStr = SettingParser.GetSingleValue
                    (settingsToSave, Column.columnRenbanHead);

                if (renbanAdd)
                {
                    // �u�A�ԁv����w�b�_�[�ɒǉ�
                    CsvHeaders = CsvHeaders.Append("�A��").ToArray();

                    int zeroPaddingDigits = int.Parse
                        (SettingParser.GetSingleValue(settingsToSave, Column.columnRenbanDigits));
                    int rowIndex = 0; // �� �w�b�_�[�s��0�ԖڂƂ��ăJ�E���g

                    reorder = (row, order) =>
                    {
                        var reordered = order.Select(i => row[i]).ToList();

                        if (rowIndex == 0)
                        {
                            // �w�b�_�[�s�ɂ́u�A�ԁv�񖼂�ǉ�
                            reordered.Add("�A��");
                        }
                        else
                        {
                            // �f�[�^�s�ɂ͘A�Ԃ�ǉ�
                            string paddedIndex = renbanAddStr+
                            rowIndex.ToString().PadLeft(zeroPaddingDigits, '0');

                            reordered.Add(paddedIndex);
                        }

                        rowIndex++;
                        return reordered.ToArray();
                    };



                    Log("�A�Ԓǉ��ݒ�F�L��");
                }
                else
                {
                    //�񏇕ύX�̃f���Q�[�g���`
                   reorder = 
                        (row, order) => order.Select(i => row[i]).ToArray();


                    Log("�A�Ԓǉ��ݒ�F����");
                }





                // CSV��ҏW���ĕۑ����邽�߂̏o�̓t�@�C�������擾
                outputName = SettingParser.GetSingleValue
                    (settingsToSave, Column.columnOutFileName);
                string outputCsvPath = Path.Combine(csvDir, outputName);

                // �������݃��O
                LogWrite(outputCsvPath);

                // CSV�̍��ڂ�ҏW���ĕۑ�
                CsvEditor.EditCsv(csvPath, outputCsvPath, columnOrder, encoding, reorder);


                // ���������i�ҏW���CSV��ΏۂɁj
                SpilitData(outputCsvPath);




                MessageBox.Show("CSV�̕ҏW�ƕۑ����������܂����B",
                    "����", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Log("CSV�ҏW�ƕۑ�����");
            }

            // ��O����
            catch (Exception ex)
            {
                MessageBox.Show($"CSV�ҏW�Ɏ��s���܂���: {ex.Message}",
                    "�G���[", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Log($"CSV�ҏW�G���[: {ex.Message}");
            }
        }


        private void SpilitData(string editedCsvPath)
        {
            //�@�g���q���������A�o�͂���t�@�C�������擾
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(outputName);

            // �����̊���擾
            // 1: �����ŕ����A2: ���Ԃŕ���
            // ����ȊO: �������Ȃ�

            // �����̊�������ȏꍇ�̃G���[�`�F�b�N
            if (!int.TryParse(SettingParser.GetSingleValue(settingsToSave, Column.columnSplitStandard),
                out splitStandard))
            {
                MessageBox.Show("�����̊�������ł��B1�i�����j�܂���2�i���ԁj���w�肵�Ă��������B",
                    "�����ݒ�G���[", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Log("�����ݒ�G���[�F�����̊������");
                return;
            }

            // �����̊�ɉ����ĕ������������s

            // �����ŕ���
            else if (splitStandard == 1)
            {
                if (!int.TryParse(SettingParser.GetSingleValue(settingsToSave, Column.columnSplitCount),
                    out splitCount))
                {
                    MessageBox.Show("���������������ł��B���l����͂��Ă��������B",
                        "���������G���[", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Log("���������G���[�F�����Ȑ��l");
                    return;
                }

                csvEditor.SplitCsvByCount(editedCsvPath, csvDir, splitCount,
                    encoding, fileNameWithoutExt);
                Log($"CSV���������F���������i{splitCount}���j");
            }

            // ���Ԃŕ���
            else if (splitStandard == 2)
            {
                if (!int.TryParse(SettingParser.GetSingleValue(settingsToSave, Column.columnSplitColumn),
                    out splitColumn))
                {
                    MessageBox.Show("�������Ԃ������ł��B���l����͂��Ă��������B",
                        "�������ԃG���[", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Log("�������ԃG���[�F�����Ȑ��l");
                    return;
                }

                csvEditor.SplitCsvByColumnValue(editedCsvPath, csvDir, splitColumn,
                    encoding, fileNameWithoutExt);
                Log($"CSV���������F���ԕ����i���� {splitColumn}�j");
            }

            // �������Ȃ�
            else
            {
                MessageBox.Show("�����̊�������ł��B1�i�����j�܂���2�i���ԁj���w�肵�Ă��������B",
                    "�����ݒ�G���[", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Log($"CSV�����Ȃ��F�����ȕ�����i{splitStandard}�j");
            }
        }




        //�@�ҏW�����ݒ�t�@�C����ۑ�����{�^���̃C�x���g�n���h��
        private void Change_SettingFile_button_Click(object sender, EventArgs e)
        {
            CollectSettingsFromGrid(); // �ҏW���e���擾

            // �Č����̎擾�Ə�����
            projectName = SettingParser.GetSingleValue(settingsToSave, Column.columnProjectName);
            if (string.IsNullOrWhiteSpace(projectName))
                projectName = Column.defoltOutputSettingFileName;

            csvPath = CSV_textBox.Text;
            csvDir = Path.GetDirectoryName(csvPath);


            // �����R�[�h���擾
            encoding = GetEncoding();

            // CSV�̃w�b�_�[��ǂݍ���
            LoadCsvHeaders();

            // ���Ԃ̑Ó����`�F�b�N
            var rawOrderValues = SettingParser.GetValues(settingsToSave, Column.columnExColumn)
                .FirstOrDefault()?.Split(':') ?? Array.Empty<string>();

            if (!IndexValidator.TryParseIndices
                (rawOrderValues, CsvHeaders.Length, out var validOrder))
                return; // �G���[���o����ۑ������𒆒f

            try
            {
                // �t�@�C�����Ɏg���Ȃ������������iWindows�̃t�@�C���������΍�j
                foreach (char c in Path.GetInvalidFileNameChars())
                    projectName = projectName.Replace(c.ToString(), "");

                // ���t��t�������t�@�C�����ɂ���
                renameOutputSettingFileName = $"{Column.timeStamp}_{projectName}.txt";
                string outputSettingPath = Path.Combine(csvDir, renameOutputSettingFileName);

                // �ݒ�t�@�C����ۑ�
                SaveSettings(settingsToSave, outputSettingPath);

                MessageBox.Show("�ݒ�t�@�C����ۑ����܂����B", "�ۑ�����", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Log($"�ҏW�ݒ�t�@�C����ۑ�: {outputSettingPath}");

                // CSV�t�@�C�����w�肳��Ă���΁ACSV�ҏW�{�^����L����
                if (!string.IsNullOrWhiteSpace(csvPath) && File.Exists(csvPath))
                    button_CSV.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"�ݒ�t�@�C���̕ۑ��Ɏ��s���܂���: {ex.Message}", "�G���[", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Log($"�ҏW�ݒ�t�@�C���ۑ��G���[: {ex.Message}");
            }
        }


        //�@DataGridView�̊e�s�����[�v���Đݒ�����W���郁�\�b�h
        //�@������g���ƁA�ҏW��̓��e��settingsToSave�ɔ��f�ł���
        void CollectSettingsFromGrid()
        {
            settingsToSave.Clear(); // �� �O��̓��e���N���A

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



        //�@�����R�[�h��Ԃ�����

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
                MessageBox.Show($"�w�肳�ꂽ�����R�[�h�u{encodingStr}�v�͖��Ή��ł��BUTF-8�ŏ������܂��B",
                    "�����R�[�h�x��", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Log($"���Ή��̕����R�[�h�w��F{encodingStr} �� UTF-8�ŏ���");
            }

            return encoding;
        }

        // DataGridView�ɐݒ��\��
        void ShowSettings(Dictionary<string, List<string>> settings)
        {
            // DataTable���g�p����DataGridView�Ƀf�[�^���o�C���h
            var table = new DataTable();
            table.Columns.Add("����");
            table.Columns.Add("�l");

            // settings�̓��e��DataTable�ɒǉ�
            foreach (var kv in settings)
            {
                string combinedValue = string.Join(", ", kv.Value);
                table.Rows.Add(kv.Key, combinedValue);
            }

            dataGridView1.DataSource = table;

        }

        // �ݒ�t�@�C����ۑ�

        public static void SaveSettings
            (Dictionary<string, List<string>> settings, string path)
        {
            //�@�ݒ�t�@�C�����������ݕۑ�
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
                // �������݃��O
                LogWrite(path);
            }

            // ��O����
            catch (Exception ex)
            {
                MessageBox.Show($"�ݒ�t�@�C���̕ۑ��Ɏ��s���܂���: {ex.Message}", "�G���[", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Log($"�ݒ�t�@�C���ۑ��G���[: {ex.Message}");
            }
        }




        // CSV�̃w�b�_�[��ǂݍ��ރ��\�b�h
        void LoadCsvHeaders()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(csvPath) || !File.Exists(csvPath))
                {
                    MessageBox.Show("CSV�t�@�C����������܂���B�p�X���m�F���Ă��������B",
                        "�t�@�C���G���[", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Log("CSV�w�b�_�[�Ǎ����s�F�t�@�C�������݂��Ȃ�");
                    CsvHeaders = Array.Empty<string>();
                    return;
                }

                LogRead(csvPath);

                var firstLine = File.ReadLines(csvPath, encoding).FirstOrDefault();
                CsvHeaders = firstLine?.Split(',') ?? Array.Empty<string>();

                if (CsvHeaders.Length == 0)
                {
                    MessageBox.Show("CSV�t�@�C���Ƀw�b�_�[��������܂���B",
                        "�w�b�_�[�G���[", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Log("CSV�w�b�_�[�Ǎ����s�F��̃w�b�_�[");
                }
                else
                {
                    Log("CSV�w�b�_�[�Ǎ�����");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"CSV�w�b�_�[�̓Ǎ��Ɏ��s���܂���: {ex.Message}",
                    "�G���[", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Log($"CSV�w�b�_�[�Ǎ��G���[: {ex.Message}");
                CsvHeaders = Array.Empty<string>();
            }
        }



        // �h���b�O�A���h�h���b�v�̃C�x���g�n���h��
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

                // CSV�t�@�C���̃p�X�ƃf�B���N�g�����擾
                csvPath = files[0];
                csvDir = Path.GetDirectoryName(csvPath);

                // CSV�t�@�C�����h���b�O�A���h�h���b�v���ꂽ�̂ŁA
                // �e���v���ݒ�t�@�C����ݒ�{�^����L����
                button_settingFile.Enabled = true;

                // ���������O�i�C�Ӂj
                Log($"CSV�t�@�C�����w�肳��܂���: {csvPath}");
            }
        }

        //�@DataGridView�̃Z���Ƀ}�E�X��������Ƃ��̃C�x���g�n���h��
        private void dataGridView1_CellToolTipTextNeeded(object sender, DataGridViewCellToolTipTextNeededEventArgs e)
        {
            toolTipSpilit.AutoPopDelay = 10000; // 10�b
            toolTipSpilit.InitialDelay = 200;   // 0.2�b�ŕ\��
            toolTipSpilit.ReshowDelay = 100;
            toolTipSpilit.ShowAlways = true;

            if (e.RowIndex >= 0 && e.ColumnIndex == 1)
            {
                string key = dataGridView1.Rows[e.RowIndex].Cells[0].Value?.ToString();
                string value = dataGridView1.Rows[e.RowIndex].Cells[1].Value?.ToString();

                if (string.IsNullOrEmpty(key))
                {
                    e.ToolTipText = $"�l: {value}";
                    return;
                }

                e.ToolTipText = itemDescriptions.ContainsKey(key)
                    ? $"{itemDescriptions[key]}\n���݂̒l: {value}"
                    : $"����: {key}\n�l: {value}";
            }
        }

        readonly Dictionary<string, string> itemDescriptions = new()
{
    { "{�����̊}", "1: �����ŕ���\n2: �w��̍��Ԃŕ���" },
    { "{�����R�[�h}", "�g�p�\: utf-8, shift_jis, utf-16" },
    { "{�A�Ԃ̌���}", "��: 6 �� 000001, 000002, ..." },
    { "{�A�Ԃ̓��̕���}", "�t�@�C�����̐擪�ɕt������鎯�ʎq" },
    { "{���o���鍀��}", "CSV�̗�ԍ��� : ��؂�Ŏw��i��: 0:2:1�j" },
    // �K�v�ɉ����Ēǉ�
};


        // DataGridView�̃Z�����_�u���N���b�N���ꂽ�Ƃ��̃C�x���g�n���h��
        void DataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {

            encoding = GetEncoding(); // �����R�[�h���擾

            LoadCsvHeaders(); // �w�b�_�[��ǂݍ���


            // �s�Ɨ�̃C���f�b�N�X���L�����m�F
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            // �_�u���N���b�N���ꂽ�s�́u���ځi�L�[�j�v���擾
            string key = dataGridView1.Rows[e.RowIndex].Cells[0].Value?.ToString();

            // ��F����̃L�[�ɑ΂��ĕҏW�t�H�[����\��
            if (key == Column.columnExColumn)
            {
                var value = dataGridView1.Rows[e.RowIndex].Cells[1].Value?.ToString();

                var editForm = new EditSettingForm(key, value, CsvHeaders); // �� �ʃt�H�[���ɓn��



                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    // �ҏW��̒l�𔽉f
                    dataGridView1.Rows[e.RowIndex].Cells[1].Value = editForm.EditedValue;

                    // settingsToSave �ɂ����f
                    if (!settingsToSave.ContainsKey(key))
                        settingsToSave[key] = new List<string>();
                    else
                        settingsToSave[key].Clear();

                    settingsToSave[key].Add(editForm.EditedValue);

                    // �ĕۑ��������Ăяo��
                    string outputSettingPath = Path.Combine(csvDir, renameOutputSettingFileName);
                    SaveSettings(settingsToSave, outputSettingPath);
                }
            }


        }

        // ���O�o�̓��\�b�h
        static void Log(string message)
        {
            string today = DateTime.Now.ToString("yyyy-MM-dd");
            string logFileName = $"log_{today}.txt";
            string logPath = Path.Combine(csvDir, logFileName);
            string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";

            try
            {
                // �t�@�C�������݂��Ȃ��ꍇ�͐V�K�쐬�i�㏑���j
                // ���݂���ꍇ�͒ǋL
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
                // ���O�o�͂Ɏ��s�����ꍇ�ł��A�v���������Ȃ��悤��
                // �\�Ȃ�A�W���o�͂�f�o�b�O�o�͂ɋL�^
                try
                {
                    System.Diagnostics.Debug.WriteLine($"���O�o�͎��s: {ex.Message}");
                }
                catch
                {
                    // Debug�o�͂����s�����ꍇ�͉������Ȃ��i���S�Ɉ���Ԃ��j
                }
            }

        }

        // �Ǎ��E�����̃��O���o�͂��郁�\�b�h
        static void LogRead(string filePath)
        {
            Log($"�Ǎ��J�n: {filePath}");
        }

        static void LogWrite(string filePath)
        {
            Log($"�������݊J�n: {filePath}");
        }



        // ���Ԃ̑Ó������`�F�b�N����N���X
        public static class IndexValidator
        {

            //�@���ڔԍ��̑Ó������`�F�b�N���郁�\�b�h
            public static bool TryParseIndices
                (IEnumerable<string> rawValues, int maxLength, out List<int> validIndices)
            {
                validIndices = new List<int>();
                var seen = new HashSet<int>();

                foreach (var val in rawValues)
                {
                    // ���Ԃ������ł��邱�Ƃ��m�F
                    // �͈͓��ł��邱�Ƃ��m�F
                    // �d�����Ă��Ȃ����Ƃ��m�F
                    if (!int.TryParse(val, out int index) ||
                        index < 0 ||
                        index >= maxLength)
                    {
                        MessageBox.Show($"���ԁu{val}�v�͖����ł��B�ݒ�ł��܂���B",
                            "���ԃG���[", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }

                    if (!seen.Add(index))
                    {
                        MessageBox.Show($"���ԁu{index}�v���d�����Ă��܂��B�d���͐ݒ�ł��܂���B",
                            "�d���G���[", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
