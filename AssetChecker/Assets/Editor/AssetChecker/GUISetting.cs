using UnityEngine;

namespace AssetChecker
{
    public class GUISetting
    {
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
    }
}