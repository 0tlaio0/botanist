// 中文卡名：示踪剂
// 卡面描述：从你的[gold]抽牌堆[/gold]中选择{Cards:diff()}张牌放入你的[gold]手牌[/gold]。
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Botanist.Scripts;

[Pool(typeof(BotanistCardPool))]
public class BotanistTracer : BotanistCardModel
{
    public override BotanistElement Element => BotanistElement.Water;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new CardsVar(2)];

    public BotanistTracer() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int selectableCount = System.Math.Min(
            DynamicVars.Cards.IntValue,
            CardPile.MaxCardsInHand - PileType.Hand.GetPile(Owner).Cards.Count);
        if (selectableCount <= 0)
        {
            return;
        }

        IEnumerable<CardModel> selected = await CardSelectCmd.FromCombatPile(
            choiceContext,
            PileType.Draw.GetPile(Owner),
            Owner,
            new CardSelectorPrefs(SelectionScreenPrompt, selectableCount));
        await CardPileCmd.Add(selected, PileType.Hand);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1m);
    }
}
