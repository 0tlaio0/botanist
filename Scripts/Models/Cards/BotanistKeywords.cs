using BaseLib.Patches.Content;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace Botanist.Scripts;

public static class BotanistKeywords
{
    /// <summary>种子成熟时结算成长效果，并通过词条说明提供1点能量。</summary>
    [CustomEnum("GROWTH")]
    [KeywordProperties(AutoKeywordPosition.None)]
    public static CardKeyword Growth = CardKeyword.None;
}
