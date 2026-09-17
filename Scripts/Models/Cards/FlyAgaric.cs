// 中文卡名：飞蝇菌子
// 卡面描述：
// 给予{VulnerablePower:diff()}层[gold]易伤[/gold]。
// [gold]成长[/gold]：造成{Damage:diff()}点伤害。
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
        $"造成{DynamicVars.Damage.IntValue}点伤害";

    public override IReadOnlyList<KeyValuePair<BotanistElement, int>> Requirements =>
    [
        new(BotanistElement.Earth, 1),
        new(BotanistElement.Wind, 1)
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(6m, ValueProp.Move),
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
        await PowerCmd.Apply<VulnerablePower>(
            choiceContext,
            SowTarget,
            DynamicVars.Vulnerable.BaseValue,
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
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}
