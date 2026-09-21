using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace Botanist.Scripts;

/// <summary>本回合打出插条时对所有敌人造成伤害。</summary>
public class BotanistStormPruningPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string? CustomPackedIconPath => BotanistArt.ElementIcon(BotanistElement.Wind);
    public override string? CustomBigIconPath => BotanistArt.ElementIcon(BotanistElement.Wind);

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner.Player ||
            !cardPlay.Card.IsCutting() ||
            CombatState is not { } combatState ||
            !cardPlay.IsLastInSeries)
        {
            return;
        }

        Flash();
        await DamageCmd.Attack(Amount)
            .TargetingAllOpponents(combatState)
            .Execute(choiceContext);
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
