using BaseLib.Patches.Content;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace Botanist.Scripts;

public static class BotanistCardTags
{
    [CustomEnum("SEED")]
    public static CardTag Seed = CardTag.None;

    [CustomEnum("CUTTING")]
    public static CardTag Cutting = CardTag.None;
}
