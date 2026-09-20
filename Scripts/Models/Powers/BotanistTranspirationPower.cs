using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace Botanist.Scripts;

/// <summary>回合结束时按本回合打出过的不同元素种类获得格挡。</summary>
public class BotanistTranspirationPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string? CustomPackedIconPath => BotanistArt.ElementIcon(BotanistElement.Wind);
    public override string? CustomBigIconPath => BotanistArt.ElementIcon(BotanistElement.Wind);

    public override async Task BeforeSideTurnEndEarly(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player ||
            !participants.Contains(Owner) ||
            Owner.IsDead ||
            Owner.Player is not { } player)
        {
            return;
        }

        int elementKinds = CombatManager.Instance.History.CardPlaysFinished
            .Where(entry =>
                entry.CardPlay.Card.Owner == player &&
                entry.HappenedThisTurn(CombatState))
            .Select(entry => entry.CardPlay.Card)
            .OfType<BotanistCardModel>()
            .Select(card => card.Element)
            .Where(element => element != BotanistElement.None)
            .Distinct()
            .Count();
        if (elementKinds <= 0)
        {
            return;
        }

        Flash();
        await CreatureCmd.GainBlock(
            Owner,
            Amount * elementKinds,
            ValueProp.Unpowered,
            null);
    }
}
