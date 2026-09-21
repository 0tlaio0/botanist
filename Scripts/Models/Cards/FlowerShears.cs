// 中文卡名：花剪
// 卡面描述：获得{Block:diff()}点[gold]格挡[/gold]。你可以消耗1张[gold]插条[/gold]：再获得4点[gold]格挡[/gold]并抽1张牌。
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Botanist.Scripts;

[Pool(typeof(BotanistCardPool))]
public class BotanistFlowerShears : BotanistCardModel
{
    public override bool GainsBlock => true;
    public override BotanistElement Element => BotanistElement.Wind;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new BlockVar(4m, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromCard<BotanistCutting>()];

    public BotanistFlowerShears()
        : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);

        bool hasCutting = PileType.Hand.GetPile(Owner).Cards.Any(BotanistCuttings.IsCutting);
        if (!hasCutting)
        {
            return;
        }

        CardSelectorPrefs prefs = new(CardSelectorPrefs.ExhaustSelectionPrompt, 0, 1)
        {
            Cancelable = true
        };
        CardModel? cutting = (await CardSelectCmd.FromHand(
            choiceContext,
            Owner,
            prefs,
            BotanistCuttings.IsCutting,
            this)).FirstOrDefault();
        if (cutting == null)
        {
            return;
        }

        await CardCmd.Exhaust(choiceContext, cutting);
        await CreatureCmd.GainBlock(Owner.Creature, 4m, ValueProp.Move, cardPlay);
        await CardPileCmd.Draw(choiceContext, 1m, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(2m);
    }
}
