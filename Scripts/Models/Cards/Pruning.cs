// 中文卡名：修枝
// 卡面描述：[gold]消耗[/gold]培养区中最后一颗[gold]种子[/gold]，抽{Cards:diff()}张牌。将该种子元素的1张[gold]插条[/gold]放到你的[gold]抽牌堆[/gold]顶。
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Botanist.Scripts;

[Pool(typeof(BotanistCardPool))]
public class BotanistPruning : BotanistCardModel
{
    public override BotanistElement Element => BotanistElement.Wind;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new CardsVar(2)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromCard<BotanistCutting>()];

    protected override bool IsPlayable =>
        BotanistCultivation.GetPlanted(Owner).Count > 0;

    protected override bool ShouldGlowGoldInternal => IsPlayable;

    public BotanistPruning() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        IReadOnlyList<PlantedSeed> planted = BotanistCultivation.GetPlanted(Owner);
        BotanistElement element = planted.Count > 0 && planted[^1].Card is BotanistCardModel botanistCard
            ? botanistCard.Element
            : Element;
        await BotanistCultivation.ConsumeLastSeed(choiceContext, Owner);
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
        await BotanistCuttings.CreateOnDrawTop(choiceContext, Owner, element);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1m);
    }
}
