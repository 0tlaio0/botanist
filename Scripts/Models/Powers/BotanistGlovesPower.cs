using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Botanist.Scripts;

/// <summary>本回合前若干张插条打出后抽牌。</summary>
public class BotanistGlovesPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string? CustomPackedIconPath => BotanistArt.ElementIcon(BotanistElement.Earth);
    public override string? CustomBigIconPath => BotanistArt.ElementIcon(BotanistElement.Earth);

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner.Player ||
            !cardPlay.Card.IsCutting() ||
            Amount <= 0 ||
            !cardPlay.IsLastInSeries)
        {
            return;
        }

        Flash();
        await CardPileCmd.Draw(choiceContext, 1m, Owner.Player);
        await PowerCmd.Decrement(this);
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Player)
        {
            await PowerCmd.Remove(this);
        }
    }
}
