// 中文卡名：林班
// 卡面描述：消耗2张[gold]插条[/gold]。获得{Energy:energyIcons()}，抽{Cards:diff()}张牌。
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Botanist.Scripts;

[Pool(typeof(BotanistCardPool))]
public class BotanistForestCrew : BotanistCardModel
{
    private const int RequiredCuttings = 2;

    public override BotanistElement Element => BotanistElement.Earth;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new EnergyVar(2),
        new CardsVar(2)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromCard<BotanistCutting>()];

    protected override bool IsPlayable =>
        Owner != null &&
        PileType.Hand.GetPile(Owner).Cards.Count(BotanistCuttings.IsCutting) >= RequiredCuttings;

    protected override bool ShouldGlowGoldInternal => IsPlayable;

    public BotanistForestCrew()
        : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        List<CardModel> cuttings = PileType.Hand.GetPile(Owner).Cards
            .Where(BotanistCuttings.IsCutting)
            .Take(RequiredCuttings)
            .ToList();
        foreach (CardModel cutting in cuttings)
        {
            await CardCmd.Exhaust(choiceContext, cutting);
        }

        await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, Owner);
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1m);
    }
}
