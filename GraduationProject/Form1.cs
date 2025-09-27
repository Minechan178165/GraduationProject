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
            = Path.Combine(exeDir, Column.defolt_settingFileName);

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
        }




        // �ݒ�t�@�C����ҏW����{�^���̃C�x���g�n���h��

        private void button_settingFile_Click(object sender, EventArgs e)
        {
            // DataGridView�Ƀe���v���[�g�ݒ��\��
            ShowSettings(settings);


            // DataGridView�̊e�s�����[�v���Đݒ�����W
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



            //// �ۑ���̐ݒ�t�@�C���p�X
            //// �Č�����������Ȃ��ꍇ�͏����l��ݒ�

            //projectName = SettingParser.GetSingleValue(settingsToSave, Column.column_projectName);

            //if (string.IsNullOrWhiteSpace(projectName))
            //{
            //    projectName = Column.defolt_output_settingFileName;
            //}



            //// CSV�t�@�C���̃p�X�ƃf�B���N�g�����擾
            //csvPath = CSV_textBox.Text;

            //csvDir = Path.GetDirectoryName(csvPath);



            //// �t�@�C�����Ɏg���Ȃ������������iWindows�̃t�@�C���������΍�j
            //foreach (char c in Path.GetInvalidFileNameChars())
            //{
            //    projectName = projectName.Replace(c.ToString(), "");
            //}

            //renameOutputSettingFileName =
            //$"{Column.timestamp}_{projectName}.txt";

            //// ���t��t�������t�@�C�����ɂ���
            //string outputSettingPath =
            //    Path.Combine(csvDir, renameOutputSettingFileName);

            //// �ݒ�t�@�C����ۑ�
            //SaveSettings(settingsToSave, outputSettingPath);

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
                orderStr = SettingParser.GetValues(settingsToSave, Column.column_exColumn)
                    .FirstOrDefault()?.Split(':');

                // �o���f�[�V�����t���ō��Ԃ����
                if (!IndexValidator.TryParseIndices
                    (orderStr ?? Array.Empty<string>(),
                    CsvHeaders.Length,
                    out var validOrder))
                    return; // �G���[���o���璆�f

                // ���Ԃ̏����𐮐��z��ɕϊ�
                columnOrder = validOrder.ToArray();



                // �񏇕ύX�̃f���Q�[�g���`
                ColumnReorderDelegate reorder = (row, order) => order.Select(i => row[i]).ToArray();

                // CSV��ҏW���ĕۑ����邽�߂̏o�̓t�@�C�������擾
                outputName = SettingParser.GetSingleValue
                    (settingsToSave, Column.column_outFileName);
                string outputCsvPath = Path.Combine(csvDir, outputName);

                // �������݃��O
                LogWrite(outputCsvPath);

                // CSV��ҏW���ĕۑ�
                CsvEditor.EditCsv(csvPath, outputCsvPath, columnOrder, encoding, reorder);

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


        //�@�ҏW�����ݒ�t�@�C����ۑ�����{�^���̃C�x���g�n���h��
        private void Change_SettingFile_button_Click(object sender, EventArgs e)
        {
            CollectSettingsFromGrid(); // �ҏW���e���擾

            // �Č����̎擾�Ə�����
            projectName = SettingParser.GetSingleValue(settingsToSave, Column.column_projectName);
            if (string.IsNullOrWhiteSpace(projectName))
                projectName = Column.defolt_output_settingFileName;

            csvPath = CSV_textBox.Text;
            csvDir = Path.GetDirectoryName(csvPath);


            // �����R�[�h���擾
            encoding = GetEncoding();

            // CSV�̃w�b�_�[��ǂݍ���
            LoadCsvHeaders();

            // ���Ԃ̑Ó����`�F�b�N
            var rawOrderValues = SettingParser.GetValues(settingsToSave, Column.column_exColumn)
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
                renameOutputSettingFileName = $"{Column.timestamp}_{projectName}.txt";
                string outputSettingPath = Path.Combine(csvDir, renameOutputSettingFileName);

                // �ݒ�t�@�C����ۑ�
                SaveSettings(settingsToSave, outputSettingPath);

                MessageBox.Show("�ݒ�t�@�C����ۑ����܂����B", "�ۑ�����", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Log($"�ҏW�ݒ�t�@�C����ۑ�: {outputSettingPath}");
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
            // �����R�[�h���擾
            encodingStr = SettingParser.GetValues
           (settingsToSave,
               Column.column_mojiCord).FirstOrDefault()?.ToLower();

            // �����R�[�h��ݒ�
            encoding = encodingStr switch
            {
                "utf-16" => Encoding.Unicode,
                "shift_jis" => Encoding.GetEncoding("shift_jis"),
                "utf-8" => Encoding.UTF8,
                _ => Encoding.UTF8 // �f�t�H���g
            };


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
            //�@CSV��ǂݍ���ŁA�w�b�_�[�݂̂��擾
            try
            {
                // �Ǎ����O
                LogRead(csvPath);

                // CSV�̃w�b�_�[��ǂݍ���
                var firstLine = File.ReadLines(csvPath, encoding).FirstOrDefault();
                CsvHeaders = firstLine?.Split(',') ?? new string[0];
                Log("CSV�w�b�_�[�Ǎ�����");
            }

            // ��O����
            catch (Exception ex)
            {
                MessageBox.Show($"CSV�w�b�_�[�̓Ǎ��Ɏ��s���܂���: {ex.Message}",
                    "�G���[", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Log($"CSV�w�b�_�[�Ǎ��G���[: {ex.Message}");
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
                target.Text = files[0];
        }






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
            if (key == Column.column_exColumn)
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
            string logPath = Path.Combine(csvDir, "log.txt");
            string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
            try
            {
                File.AppendAllText(logPath, logEntry + Environment.NewLine, Encoding.UTF8);
            }
            catch
            {
                // ���O�o�͂Ɏ��s���Ă��A�v���������Ȃ��悤��
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
    }

}
