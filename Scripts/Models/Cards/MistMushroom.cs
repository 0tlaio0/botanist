// 中文卡名：雾菇
// 卡面描述：
// 给予1层[gold]虚弱[/gold]。
// [gold]成长[/gold]：造成{Damage:diff()}点伤害并给予{WeakPower:diff()}层[gold]虚弱[/gold]。
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Botanist.Scripts;

[Pool(typeof(BotanistCardPool))]
public class BotanistMistMushroom : BotanistTargetedSeedCardModel
{
    public override BotanistElement Element => BotanistElement.Earth;

    public override string RipenSummary =>
        $"造成{DynamicVars.Damage.IntValue}点伤害并给予{DynamicVars.Weak.IntValue}层虚弱";

    public override IReadOnlyList<KeyValuePair<BotanistElement, int>> Requirements =>
    [
        new(BotanistElement.Earth, 1),
        new(BotanistElement.Water, 1)
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(7m, ValueProp.Move),
        new PowerVar<WeakPower>(1m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<WeakPower>()];

    public BotanistMistMushroom() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, true)
    {
    }

    protected override async Task OnSow(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        await PowerCmd.Apply<WeakPower>(
            choiceContext,
            SowTarget,
            1m,
            Owner.Creature,
            this);
    }

    public override async Task OnRipen(PlayerChoiceContext choiceContext)
    {
        if (RipenTarget is not { } target)
        {
            return;
        }

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(target)
            .Execute(choiceContext);

        if (target.IsAlive)
        {
            await PowerCmd.Apply<WeakPower>(
                choiceContext,
                target,
                DynamicVars.Weak.BaseValue,
                Owner.Creature,
                this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Weak.UpgradeValueBy(1m);
    }
}
