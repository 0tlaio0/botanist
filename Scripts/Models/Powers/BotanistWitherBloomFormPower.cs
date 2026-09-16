using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Botanist.Scripts;

/// <summary>强化种子入场联动，并在本回合没有种子成长时结算损失生命。</summary>
public class BotanistWitherBloomFormPower : CustomPowerModel
{
    private const string HpLossKey = "HpLoss";

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;
    public override string? CustomPackedIconPath => BotanistArt.ElementIcon(BotanistElement.Aether);
    public override string? CustomBigIconPath => BotanistArt.ElementIcon(BotanistElement.Aether);

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(HpLossKey, 10m, DamageProps.nonCardHpLoss)];

    public void SetHpLoss(decimal amount)
    {
        AssertMutable();
        DynamicVars[HpLossKey].BaseValue = amount;
    }

    public override async Task BeforeSideTurnEndVeryEarly(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player ||
            !participants.Contains(Owner) ||
            Owner.IsDead ||
            Owner.Player is not { } player ||
            BotanistCultivation.GetSeedsCultivatedThisTurn(player) > 0)
        {
            return;
        }

        Flash();
        await CreatureCmd.Damage(
            choiceContext,
            Owner,
            DynamicVars[HpLossKey].BaseValue,
            DamageProps.nonCardHpLoss,
            null,
            null);
    }
}
