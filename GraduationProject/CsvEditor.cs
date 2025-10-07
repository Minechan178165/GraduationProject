using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GraduationProject.Form1;

namespace GraduationProject
{
    // CSV編集用クラス
    // 列順変更用デリゲート
    public delegate string[] ColumnReorderDelegate(string[] row, int[] order);
    internal class CsvEditor
    {
        public static void EditCsv(string inputPath, string outputPath,
               int[] columnOrder, Encoding encoding, ColumnReorderDelegate reorder)
        {
            var lines = File.ReadAllLines(inputPath, encoding);
            var edited = lines.Select(line => reorder(line.Split(','), columnOrder))
                              .Select(cols => string.Join(",", cols));

            File.WriteAllLines(outputPath, edited, encoding);
        }

        //　データを分割
        //　ファイル名を設定できる用意する

        //　件数で分割
        public void SplitCsvByCount
            (string inputCsvPath, string outputDir, int chunkSize, 
            Encoding encoding,string outputFileName)
        {
            //　全行を読み込み、ヘッダーとデータに分割
            var allLines = File.ReadAllLines(inputCsvPath, encoding);
            var header = allLines.First();
            var rows = allLines.Skip(1).ToList();

            
            //　チャンク数を計算
            int totalChunks = (int)Math.Ceiling((double)rows.Count / chunkSize);

            //　チャンクごとにファイルを作成
            for (int i = 0; i < totalChunks; i++)
            {
                var chunkRows = rows.Skip(i * chunkSize).Take(chunkSize);
                var outputPath = Path.Combine(outputDir, outputFileName+$"_chunk_{i + 1}.csv");
                File.WriteAllLines(outputPath, new[] { header }.Concat(chunkRows), encoding);
                //Debug.LogWrite(outputPath);
            }
        }

        //　項番で分割
        public void SplitCsvByColumnValue
            (string inputCsvPath, string outputDir, int keyColumnIndex, 
            Encoding encoding, string outputFileName)
        {
            //　全行を読み込み、ヘッダーとデータに分割
            var allLines = File.ReadAllLines(inputCsvPath, encoding);
            var header = allLines.First();
            var rows = allLines.Skip(1);

            //　指定列の値でグループ化
            var grouped = rows
                .Select(line => new { Line = line, Key = line.Split(',')[keyColumnIndex] })
                .GroupBy(x => x.Key);

            //　グループごとにファイルを作成
            foreach (var group in grouped)
            {
                var outputPath = Path.Combine(outputDir, outputFileName+$"_group_{group.Key}.csv");
                File.WriteAllLines(outputPath, new[] { header }
                .Concat(group.Select(x => x.Line)), encoding);
                //Debug.LogWrite(outputPath);
            }
        }

        
    }
}
