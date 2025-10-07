using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraduationProject
{
    internal class IndexValidator
    {

        // 項番の妥当性をチェックするクラス
        //public static class IndexValidator


        //　項目番号の妥当性をチェックするメソッド
        public static bool TryParseIndices
            (IEnumerable<string> rawValues, int maxLength, out List<int> validIndices)
        {
            validIndices = new List<int>();
            var seen = new HashSet<int>();

            foreach (var val in rawValues)
            {
                // 項番が整数であることを確認
                // 範囲内であることを確認
                // 重複していないことを確認
                if (!int.TryParse(val, out int index) ||
                    index < 0 ||
                    index >= maxLength)
                {
                    MessageBox.Show($"項番「{val}」は無効です。設定できません。",
                        "項番エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (!seen.Add(index))
                {
                    MessageBox.Show($"項番「{index}」が重複しています。重複は設定できません。",
                        "重複エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }



                validIndices.Add(index);
            }

            return true;
        }
    }

}

