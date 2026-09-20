// 中文卡名：藤蔓蹒跚者
// 卡面描述：
// 造成{Damage:diff()}点伤害2次。
// [gold]成长[/gold]：所有敌人在本回合失去{StrengthLoss:diff()}点[gold]力量[/gold]。
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Botanist.Scripts;

[Pool(typeof(BotanistCardPool))]
public class BotanistVineStaggerer : BotanistTargetedSeedCardModel
{
    private const string StrengthLossKey = "StrengthLoss";

    public override BotanistElement Element => BotanistElement.Water;

    public override string RipenSummary =>
        $"所有敌人在本回合失去{DynamicVars[StrengthLossKey].IntValue}点力量";

    public override IReadOnlyList<KeyValuePair<BotanistElement, int>> Requirements =>
    [
        new(BotanistElement.Water, 1),
        new(BotanistElement.Earth, 2),
        new(BotanistElement.Fire, 1)
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(5m, ValueProp.Move),
        new DynamicVar(StrengthLossKey, 6m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<BotanistStaggerPower>()];

    public BotanistVineStaggerer() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, true)
    {
    }

    protected override async Task OnSow(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .WithHitCount(2)
            .FromCard(this)
            .Targeting(SowTarget)
            .Execute(choiceContext);
    }

    public override async Task OnRipen(PlayerChoiceContext choiceContext)
    {
        if (RipenCombatState is not { } combatState)
        {
            return;
        }

        foreach (var enemy in combatState.HittableEnemies)
        {
            await PowerCmd.Apply<BotanistStaggerPower>(
                choiceContext,
                enemy,
                DynamicVars[StrengthLossKey].BaseValue,
                Owner.Creature,
                this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars[StrengthLossKey].UpgradeValueBy(3m);
    }
}
