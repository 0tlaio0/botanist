using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Botanist.Scripts;

/// <summary>本回合受到的伤害减少25%，持续到敌方回合结束。</summary>
public class BotanistThickEpidermisPower : CustomPowerModel
{
    private const string DamageDecreaseKey = "DamageDecrease";

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override string? CustomPackedIconPath => BotanistArt.ElementIcon(BotanistElement.Earth);
    public override string? CustomBigIconPath => BotanistArt.ElementIcon(BotanistElement.Earth);

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar(DamageDecreaseKey, 25m)];

    public override decimal ModifyDamageMultiplicative(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target != Owner)
        {
            return 1m;
        }

        return (100m - DynamicVars[DamageDecreaseKey].BaseValue) / 100m;
    }

    public override Task AfterModifyingDamageAmount(CardModel? cardSource)
    {
        Flash();
        return Task.CompletedTask;
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Enemy)
        {
            await PowerCmd.Remove(this);
        }
    }
}
