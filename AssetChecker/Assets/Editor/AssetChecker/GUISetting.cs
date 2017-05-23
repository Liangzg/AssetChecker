using UnityEngine;

namespace AssetChecker
{
    public class GUISetting
    {
        public static string SelectFilePath;
        public static Color SelectColor = NGUIText.ParseColor32("0088CCFF", 0);

        public const string TOP_BAR_LABEL_STYLE_NAME = "TopBarLabel";
        public const string TOP_BAR_BTN_STYLE_NAME = "TopBarButton";
        public const string TOP_BAR_BG_STYLE_NAME = "TopBarBg";

        //评级
        public static Color[] ScoreColors = new[]
        {
            NGUIText.ParseColor32("17F133FF",0),
            NGUIText.ParseColor32("87FF7DFF",0),
            NGUIText.ParseColor32("F3FF36FF" , 0),
            NGUIText.ParseColor32("B50404FF" , 0),
            NGUIText.ParseColor32("590404FF" , 0)
        };
        public static string[] ScoreNames = new[] {"完美", "优秀", "合格", "超标", "你咋不上天了"};
        /// <summary>
        /// 计算评分
        /// </summary>
        /// <param name="score"></param>
        /// <returns></returns>
        public static int CalScoreLevel(float score)
        {
            int offset = (int)((score - 1) * 100);
            offset = offset == 0 ? 1 : offset;
            offset = offset / Mathf.Abs(offset);  //计算正负
            int lv = score > 1 ? 1 : 0;

            float offsetScore = Mathf.Abs(score - 1f);
            if (offsetScore >= 0.29f)   lv = 2;
            else if (offsetScore >= 0.19f)  lv = 1; 

            lv = 2 + lv*offset;
            return (int)Mathf.Clamp(lv , 0 , 4);
        }
    }
}