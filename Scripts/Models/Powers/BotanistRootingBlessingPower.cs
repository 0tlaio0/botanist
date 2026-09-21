using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Botanist.Scripts;

/// <summary>打出种子时获得格挡，回合结束时再按培养区种子数获得格挡。</summary>
public class BotanistRootingBlessingPower : CustomPowerModel
{
    private const string EndBlockKey = "EndBlock";

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string? CustomPackedIconPath => BotanistArt.ElementIcon(BotanistElement.Earth);
    public override string? CustomBigIconPath => BotanistArt.ElementIcon(BotanistElement.Earth);

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar(EndBlockKey, 3m)];

    public void SetEndBlock(decimal amount)
    {
        AssertMutable();
        DynamicVars[EndBlockKey].BaseValue = amount;
    }

    public void AddEndBlock(decimal amount)
    {
        AssertMutable();
        DynamicVars[EndBlockKey].BaseValue += amount;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner.Player || !cardPlay.Card.IsSeed())
        {
            return;
        }

        Flash();
        await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Unpowered, null, fast: true);
    }

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
            DynamicVars[EndBlockKey].BaseValue * seedCount,
            ValueProp.Unpowered,
            null);
    }
}