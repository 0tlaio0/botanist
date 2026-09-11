using BaseLib.Patches.Content;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace Qgs.Scripts;

public static class QgsKeywords
{
    [CustomEnum("GROWTH")]
    [KeywordProperties(AutoKeywordPosition.None)]
    public static CardKeyword Growth = CardKeyword.None;
}
