using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Qgs.Scripts;

public abstract class QgsSeedCardModel : QgsCardModel
{
    public abstract IReadOnlyList<KeyValuePair<QgsElement, int>> Requirements { get; }

    public abstract string RipenSummary { get; }

    protected QgsSeedCardModel(int energyCost, CardRarity rarity, TargetType targetType, bool shouldShowInCardLibrary)
        : base(energyCost, CardType.Power, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override PileType GetResultPileTypeForCardPlay()
    {
        if (QgsCultivation.HasSpace(Owner))
        {
            return PileType.Play;
        }

        return PileType.Discard;
    }

    public abstract Task OnRipen(PlayerChoiceContext choiceContext);
}
