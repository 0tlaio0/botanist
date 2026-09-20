// 中文卡名：精准灌溉
// 卡面描述：使培育区中每颗[gold]种子[/gold]当前最高的一种元素需求减少{IfUpgraded:show:X+1|X}。
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Botanist.Scripts;

[Pool(typeof(BotanistCardPool))]
public class BotanistPrecisionIrrigation : BotanistCardModel
{
    public override BotanistElement Element => BotanistElement.Water;
    protected override bool HasEnergyCostX => true;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];

    public BotanistPrecisionIrrigation()
        : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int amount = ResolveEnergyXValue() + (IsUpgraded ? 1 : 0);
        await BotanistCultivation.ReduceHighestSeedRequirements(
            choiceContext,
            Owner,
            amount);
    }

    protected override void OnUpgrade()
    {
    }
}
