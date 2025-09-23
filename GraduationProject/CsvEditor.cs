using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GraduationProject.Form1;

namespace GraduationProject
{
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
    }
}
