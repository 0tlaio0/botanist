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

/// <summary>敌人回合结束时受到伤害，并按实际损失的生命值为施术者回复生命。</summary>
public class BotanistParasitePower : CustomPowerModel
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string? CustomPackedIconPath => BotanistArt.ElementIcon(BotanistElement.Water);
    public override string? CustomBigIconPath => BotanistArt.ElementIcon(BotanistElement.Water);

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (!participants.Contains(Owner) || Owner.IsDead)
        {
            return;
        }

        Flash();
        IEnumerable<DamageResult> results = await CreatureCmd.Damage(
            choiceContext,
            Owner,
            Amount,
            ValueProp.Unpowered,
            Applier,
            null);

        int lifeLost = results.Sum(result => result.UnblockedDamage);
        if (lifeLost <= 0 || Applier is not { IsAlive: true } applier)
        {
            return;
        }

        await CreatureCmd.Heal(applier, lifeLost);
    }
}
