// 中文卡名：狂野生长
// 卡面描述：培养区中每有1颗[gold]种子[/gold]，造成{Damage:diff()}点伤害1次。
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Botanist.Scripts;

[Pool(typeof(BotanistCardPool))]
public class BotanistWildGrowth : BotanistCardModel
{
    public override BotanistElement Element => BotanistElement.Fire;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(4, ValueProp.Move)];

    public BotanistWildGrowth() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        int seedCount = BotanistCultivation.GetPlanted(Owner).Count;
        if (seedCount <= 0)
        {
            return;
        }

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .WithHitCount(seedCount)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
    }
}