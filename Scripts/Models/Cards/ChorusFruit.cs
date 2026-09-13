// 中文卡名：紫颂果
// 卡面描述：将你[gold]弃牌堆[/gold]中的牌重新洗牌放入[gold]抽牌堆[/gold]，抽{Cards:diff()}张牌，获得{Energy:diff()}点能量。当这张牌被保留时变为[gold]爆裂紫颂果[/gold]。
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace Botanist.Scripts;

[Pool(typeof(TokenCardPool))]
public class BotanistChorusFruit : BotanistCardModel
{
    public override bool CanBeGeneratedInCombat => false;
    public override BotanistElement Element => BotanistElement.Wind;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Retain,
        CardKeyword.Exhaust
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(2),
        new EnergyVar(1)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        base.EnergyHoverTip,
        HoverTipFactory.FromCard<BotanistExplosiveChorusFruit>(IsUpgraded)
    ];

    public BotanistChorusFruit() : base(0, CardType.Skill, CardRarity.Token, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CardPileCmd.Shuffle(choiceContext, Owner);
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
        await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);
    }

    public override async Task AfterFlush(
        PlayerChoiceContext choiceContext,
        Player player,
        IReadOnlyCollection<CardModel> flushedCards,
        IReadOnlyCollection<CardModel> retainedCards)
    {
        if (player != Owner || !retainedCards.Contains(this) || CombatState == null)
        {
            return;
        }

        BotanistExplosiveChorusFruit explosive = CombatState.CreateCard<BotanistExplosiveChorusFruit>(Owner);
        if (IsUpgraded)
        {
            CardCmd.Upgrade(explosive, CardPreviewStyle.None);
        }

        await CardCmd.Transform(this, explosive);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Energy.UpgradeValueBy(1m);
    }
}
