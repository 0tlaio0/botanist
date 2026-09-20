// 中文卡名：夹竹桃
// 卡面描述：
// 给予所有敌人1层[gold]虚弱[/gold]。
// [gold]成长[/gold]：对所有敌人施加{PoisonPower:diff()}层[gold]中毒[/gold]。
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Botanist.Scripts;

[Pool(typeof(BotanistCardPool))]
public class BotanistOleander : BotanistSeedCardModel
{
    public override BotanistElement Element => BotanistElement.Water;

    public override string RipenSummary =>
        $"对所有敌人施加{DynamicVars.Poison.IntValue}层中毒";

    public override IReadOnlyList<KeyValuePair<BotanistElement, int>> Requirements =>
    [
        new(BotanistElement.Water, 1),
        new(BotanistElement.Earth, 1)
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<PoisonPower>(5m),
        new PowerVar<WeakPower>(1m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<PoisonPower>(),
        HoverTipFactory.FromPower<WeakPower>()
    ];

    public BotanistOleander()
        : base(1, CardType.Skill, CardRarity.Common, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (CombatState is not { } combatState)
        {
            return;
        }

        await PowerCmd.Apply<WeakPower>(
            choiceContext,
            combatState.HittableEnemies,
            DynamicVars.Weak.BaseValue,
            Owner.Creature,
            this);
    }

    public override async Task OnRipen(PlayerChoiceContext choiceContext)
    {
        if (RipenCombatState is not { } combatState)
        {
            return;
        }

        await PowerCmd.Apply<PoisonPower>(
            choiceContext,
            combatState.HittableEnemies,
            DynamicVars.Poison.BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Poison.UpgradeValueBy(3m);
    }
}
