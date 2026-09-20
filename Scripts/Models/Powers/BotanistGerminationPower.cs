using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Botanist.Scripts;

/// <summary>令接下来打出的指定数量种子牌耗能变为0。</summary>
public class BotanistGerminationPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string? CustomPackedIconPath => BotanistArt.ElementIcon(BotanistElement.Water);
    public override string? CustomBigIconPath => BotanistArt.ElementIcon(BotanistElement.Water);

    public override bool TryModifyEnergyCostInCombat(
        CardModel card,
        decimal originalCost,
        out decimal modifiedCost)
    {
        bool applies = Amount > 0 && card.Owner == Owner.Player && card.IsSeed();
        modifiedCost = applies ? 0m : originalCost;
        return applies && modifiedCost != originalCost;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner.Player ||
            !cardPlay.Card.IsSeed() ||
            !cardPlay.IsLastInSeries)
        {
            return;
        }

        Flash();
        await PowerCmd.Decrement(this);
    }
}
