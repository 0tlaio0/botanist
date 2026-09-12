using BaseLib.Patches.Content;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace Botanist.Scripts;

public static class BotanistKeywords
{
    [CustomEnum("GROWTH")]
    [KeywordProperties(AutoKeywordPosition.None)]
    public static CardKeyword Growth = CardKeyword.None;
}
