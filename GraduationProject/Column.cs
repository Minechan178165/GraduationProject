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

        //　設定ファイル：分割の基準
        public static readonly string columnSplitStandard =
            "{分割の基準}";

        //　設定ファイル：分割する項番
        public static readonly string columnSplitColumn =
            "{分割の指定の項番}";

        //　設定ファイル：分割する件数
        public static readonly string columnSplitCount =
            "{分割する件数}";




        //　設定ファイル：連番を付加する必要の有無
        public static readonly string columnRenbanNeed = 
            "{連番を末尾に付加するか}";
          

        //　設定ファイル：連番の桁数
        public static readonly string columnRenbanDigits =
            "{連番の桁数}";

        //　設定ファイル：連番の頭の文字
        public static readonly string columnRenbanHead =
            "{連番の頭の文字}";



    }
}
