using MegaCrit.Sts2.Core.Entities.Cards;
using BaseLib.Abstracts;

namespace Qgs.Scripts;

public abstract class QgsCardModel : CustomCardModel
{
    public abstract QgsElement Element { get; }

    protected QgsCardModel(int energyCost, CardType type, CardRarity rarity, TargetType targetType, bool shouldShowInCardLibrary)
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }
}
