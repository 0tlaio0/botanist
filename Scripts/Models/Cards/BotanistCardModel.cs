using MegaCrit.Sts2.Core.Entities.Cards;
using BaseLib.Abstracts;

namespace Botanist.Scripts;

public abstract class BotanistCardModel : CustomCardModel
{
    public abstract BotanistElement Element { get; }
    public override string PortraitPath => BotanistArt.Character;

    protected BotanistCardModel(int energyCost, CardType type, CardRarity rarity, TargetType targetType, bool shouldShowInCardLibrary)
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }
}
