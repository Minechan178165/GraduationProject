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
        string csvDir;


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



            // �ۑ���̐ݒ�t�@�C���p�X
            // �Č�����������Ȃ��ꍇ�͏����l��ݒ�

            projectName = SettingParser.GetSingleValue(settingsToSave, Column.column_projectName);

            if (string.IsNullOrWhiteSpace(projectName))
            {
                projectName = Column.defolt_output_settingFileName;
            }



            // CSV�t�@�C���̃p�X�ƃf�B���N�g�����擾
            csvPath = CSV_textBox.Text;

            csvDir = Path.GetDirectoryName(csvPath);



            // �t�@�C�����Ɏg���Ȃ������������iWindows�̃t�@�C���������΍�j
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                projectName = projectName.Replace(c.ToString(), "");
            }

            renameOutputSettingFileName =
            $"{Column.timestamp}_{projectName}.txt";

            // ���t��t�������t�@�C�����ɂ���
            string outputSettingPath =
                Path.Combine(csvDir, renameOutputSettingFileName);

            // �ݒ�t�@�C����ۑ�
            SaveSettings(settingsToSave, outputSettingPath);

        }

        // CSV�t�@�C����ҏW����{�^��
        // �V�����ݒ�t�@�C����ǂݍ����CSV��ҏW�E�ۑ�����{�^���̃C�x���g�n���h��

        private void button_CSV_Click_1(object sender, EventArgs e)
        {
            // �����R�[�h���擾
            encoding = GetEncoding();


            LoadCsvHeaders(); // �w�b�_�[��ǂݍ���

            ////�@CSV�Ɠ����t�H���_�ɂ���ݒ�t�@�C�����g�p

            string editedSettingPath =
                Path.Combine(csvDir, renameOutputSettingFileName);

            // �ݒ�t�@�C����ǂݍ���
            var settings = SettingParser.ParseMultiple(editedSettingPath);



            // �񏇕ύX�̃��W�b�N

            //�@���ړ���ւ��̏������擾
            orderStr =
            SettingParser.GetValues
            (settingsToSave,
                Column.column_exColumn).FirstOrDefault()?.Split(':');

            columnOrder = orderStr?.Select(s => int.Parse(s)).ToArray() ?? new int[0];


            ColumnReorderDelegate reorder = (row, order) => order.Select(i => row[i]).ToArray();

            // CSV��ҏW���ĕۑ�

            //�@�o�͂���t�@�C�������擾
            outputName =
            SettingParser.GetSingleValue(settingsToSave, Column.column_outFileName);


            CsvEditor.EditCsv(csvPath, Path.Combine(csvDir, outputName),
                columnOrder, encoding, reorder);

            MessageBox.Show("CSV�̕ҏW�ƕۑ����������܂����B",
                "����", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }


        //�@�����R�[�h��Ԃ�����

        Encoding GetEncoding()
        {
            encodingStr = SettingParser.GetValues
           (settingsToSave,
               Column.column_mojiCord).FirstOrDefault()?.ToLower();

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
            var table = new DataTable();
            table.Columns.Add("����");
            table.Columns.Add("�l");

            foreach (var kv in settings)
            {
                string combinedValue = string.Join(", ", kv.Value);
                table.Rows.Add(kv.Key, combinedValue);
            }

            dataGridView1.DataSource = table;
        }

        // �ݒ�t�@�C����ۑ�
        public static void SaveSettings(Dictionary<string, List<string>> settings, string path)
        {
            using (var writer = new StreamWriter(path, false, Encoding.UTF8))
            {
                foreach (var kv in settings)
                {
                    foreach (var value in kv.Value)
                        writer.WriteLine($"{kv.Key}:{value}");
                }
            }
        }

        // CSV�̃w�b�_�[��ǂݍ��ރ��\�b�h
        void LoadCsvHeaders()
        {
            var firstLine = File.ReadLines(csvPath, encoding).FirstOrDefault();
            CsvHeaders = firstLine?.Split(',') ?? new string[0];
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

                //if (editForm.ShowDialog() == DialogResult.OK)
                //{
                //    // �ҏW��̒l�𔽉f
                //    dataGridView1.Rows[e.RowIndex].Cells[1].Value = editForm.EditedValue;
                //}

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

        void SaveCurrentSettings()
        {
            if (string.IsNullOrEmpty(csvDir)) return;

            string outputSettingPath = Path.Combine(csvDir, renameOutputSettingFileName);
            SaveSettings(settingsToSave, outputSettingPath);
        }

    }
}
