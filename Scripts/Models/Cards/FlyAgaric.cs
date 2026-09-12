// 中文卡名：飞蝇菌子
// 卡面描述：
// 造成{Damage:diff()}点伤害。
// [gold]成长[/gold]：造成{RipenDamage:diff()}点伤害并给予{VulnerablePower:diff()}层[gold]易伤[/gold]。
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
public class BotanistFlyAgaric : BotanistTargetedSeedCardModel
{
    public override BotanistElement Element => BotanistElement.Earth;

    public override string RipenSummary =>
        $"造成{DynamicVars["RipenDamage"].IntValue}点伤害并给予{DynamicVars.Vulnerable.IntValue}层易伤";

    public override IReadOnlyList<KeyValuePair<BotanistElement, int>> Requirements =>
    [
        new(BotanistElement.Earth, 1),
        new(BotanistElement.Wind, 1)
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(4m, ValueProp.Move),
        new DamageVar("RipenDamage", 6m, ValueProp.Move),
        new PowerVar<VulnerablePower>(1m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<VulnerablePower>()];

    public BotanistFlyAgaric() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, true)
    {
    }

    protected override async Task OnSow(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(SowTarget)
            .Execute(choiceContext);
    }

    public override async Task OnRipen(PlayerChoiceContext choiceContext)
    {
        if (RipenTarget is not { } target)
        {
            return;
        }

        await DamageCmd.Attack(DynamicVars["RipenDamage"].BaseValue)
            .FromCard(this)
            .Targeting(target)
            .Execute(choiceContext);

        if (target.IsAlive)
        {
            await PowerCmd.Apply<VulnerablePower>(
                choiceContext,
                target,
                DynamicVars.Vulnerable.BaseValue,
                Owner.Creature,
                this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Vulnerable.UpgradeValueBy(1m);
    }
}
