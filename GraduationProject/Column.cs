using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraduationProject
{
    //　変更できない値をまとめたクラス
    public static class Column
    {
        //　設定ファイルのデフォルト名
        public static readonly string defoltSettingFileName = 
            "defoltSettingFile.txt";

        // 案件名が見つからない場合の初期値
        public static readonly string defoltOutputSettingFileName = 
            "出力_設定ファイル";


        //　今日の日付
        public static readonly string timeStamp =
            DateTime.Now.ToString("yyyyMMdd");


        //　設定ファイル：案件名
        public static readonly string columnProjectName =
            "{案件名}";

        //　設定ファイル：出力するファイル名
        public static readonly string columnOutFileName =
            "{出力するファイル名}";

        //　設定ファイル：抽出する項番
        public static readonly string columnExColumn =
            "{抽出する項番}";

        //　設定ファイル：文字コード
        public static readonly string columnMojiCord =
            "{文字コード}";

    }
}
