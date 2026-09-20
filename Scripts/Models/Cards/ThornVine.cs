// 中文卡名：荆棘藤蔓
// 卡面描述：
// 给予所有敌人{WeakPower:diff()}层[gold]虚弱[/gold]。
// [gold]成长[/gold]：给予{VulnerablePower:diff()}层[gold]易伤[/gold]。
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
public class BotanistThornVine : BotanistSeedCardModel
{
    public override BotanistElement Element => BotanistElement.Earth;

    public override string RipenSummary =>
        $"给予所有敌人{DynamicVars.Vulnerable.IntValue}层易伤";

    public override IReadOnlyList<KeyValuePair<BotanistElement, int>> Requirements =>
    [
        new(BotanistElement.Earth, 2),
        new(BotanistElement.Fire, 2),
        new(BotanistElement.Water, 1)
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<WeakPower>(2m),
        new PowerVar<VulnerablePower>(2m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<VulnerablePower>()
    ];

    public BotanistThornVine()
        : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies, true)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
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

        await PowerCmd.Apply<VulnerablePower>(
            choiceContext,
            combatState.HittableEnemies,
            DynamicVars.Vulnerable.BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Weak.UpgradeValueBy(1m);
        DynamicVars.Vulnerable.UpgradeValueBy(1m);
    }
}
