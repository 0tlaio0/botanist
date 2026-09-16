using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace Botanist.Scripts;

/// <summary>回合结束时按培养区中的种子数量提供格挡。</summary>
public class BotanistHedgePower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string? CustomPackedIconPath => BotanistArt.ElementIcon(BotanistElement.Earth);
    public override string? CustomBigIconPath => BotanistArt.ElementIcon(BotanistElement.Earth);

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

        int seedCount = BotanistCultivation.GetPlanted(player).Count;
        if (seedCount == 0)
        {
            return;
        }

        Flash();
        await CreatureCmd.GainBlock(
            Owner,
            Amount * seedCount,
            ValueProp.Unpowered,
            null);
    }
}
