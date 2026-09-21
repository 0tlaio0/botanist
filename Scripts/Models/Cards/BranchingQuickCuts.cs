// 中文卡名：连枝快切
// 卡面描述：造成{Damage:diff()}点伤害2次。若本回合打出过[gold]插条[/gold]，额外攻击{Repeat:diff()}次。
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
public class BotanistBranchingQuickCuts : BotanistCardModel
{
    private const int BaseHitCount = 2;

    public override BotanistElement Element => BotanistElement.Fire;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(4m, ValueProp.Move),
        new RepeatVar(1)
    ];

    protected override bool ShouldGlowGoldInternal =>
        BotanistCultivation.GetCuttingsPlayedThisTurn(Owner) > 0;

    public BotanistBranchingQuickCuts()
        : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        int hitCount = BaseHitCount;
        if (BotanistCultivation.GetCuttingsPlayedThisTurn(Owner) > 0)
        {
            hitCount += DynamicVars.Repeat.IntValue;
        }

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .WithHitCount(hitCount)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Repeat.UpgradeValueBy(1m);
    }
}
