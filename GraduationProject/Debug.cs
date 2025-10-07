using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraduationProject
{
    public static class Debug
    {

        public static string csvDir { get; set; }
        // ログ出力メソッド
        public static void Log(string message)
        {
            string today = DateTime.Now.ToString("yyyy-MM-dd");
            string LogFileName = $"Log_{today}.txt";
            string LogPath = Path.Combine(csvDir, LogFileName);
            string LogEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";

            try
            {
                // ファイルが存在しない場合は新規作成（上書き）
                // 存在する場合は追記
                if (!File.Exists(LogPath))
                {
                    File.WriteAllText(LogPath, LogEntry + Environment.NewLine, Encoding.UTF8);

                }
                else
                {
                    File.AppendAllText(LogPath, LogEntry + Environment.NewLine, Encoding.UTF8);
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
        public static void LogRead(string filePath)
        {
            Log($"読込開始: {filePath}");
        }

        public static void LogWrite(string filePath)
        {
            Log($"書き込み開始: {filePath}");
        }
    }
}
