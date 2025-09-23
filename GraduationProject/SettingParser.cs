using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraduationProject
{
      // 設定ファイル読み込みクラス（複数値対応）
        public static class SettingParser
        {
        /// <summary>
        /// 設定ファイルのキーと値を
        ///  Dictionary<string, List<string>> にパースする
        ///  
        public static Dictionary<string, List<string>> ParseMultiple(string path)
            {
                var dict = new Dictionary<string, List<string>>();

                foreach (var line in File.ReadAllLines(path, Encoding.UTF8))
                {
                    // 最初の ":" だけで分割
                    int index = line.IndexOf(':');
                    if (index == -1) continue;

                    string key = line.Substring(0, index).Trim();
                    string value = line.Substring(index + 1).Trim();

                    if (!dict.ContainsKey(key))
                        dict[key] = new List<string>();

                    dict[key].Add(value);
                }

                return dict;
            }

        /// <summary>
        ///  設定ファイルから値を取得するヘルパー関数
        ///  
        public static string GetSingleValue(Dictionary<string, List<string>> settings, string key)
            {
                if (settings.TryGetValue(key, out var values) && values.Count > 0)
                    return values[0];
                return string.Empty;
            }

            public static List<string> GetValues(Dictionary<string, List<string>> settings, string key)
            {
                return settings.TryGetValue(key, out var values) ? values : new List<string>();
            }


        }
    }
