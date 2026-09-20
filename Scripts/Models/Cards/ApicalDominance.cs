// 中文卡名：顶端优势
// 卡面描述：[gold]消耗[/gold]1张[gold]种子[/gold]牌，使抽牌堆中随机1张[gold]种子[/gold]牌成长需求减少1。
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Botanist.Scripts;

[Pool(typeof(BotanistCardPool))]
public class BotanistApicalDominance : BotanistCardModel
{
    public override BotanistElement Element => BotanistElement.Earth;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];

    protected override bool IsPlayable =>
        PileType.Hand.GetPile(Owner).Cards.Any(card => card.IsSeed());

    protected override bool ShouldGlowGoldInternal => IsPlayable;

    public BotanistApicalDominance() : base(2, CardType.Skill, CardRarity.Rare, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        CardModel? consumedSeed = (await CardSelectCmd.FromHand(
            choiceContext,
            Owner,
            new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, 1),
            filter: card => card.IsSeed(),
            source: this)).FirstOrDefault();

        if (consumedSeed == null)
        {
            return;
        }

        await CardCmd.Exhaust(choiceContext, consumedSeed);

        CardModel? targetSeed = PileType.Draw.GetPile(Owner).Cards
            .Where(card => card.IsSeed())
            .TakeRandom(1, Owner.RunState.Rng.CombatCardSelection)
            .FirstOrDefault();
        if (targetSeed != null)
        {
            BotanistCultivation.ReduceSeedRequirement(targetSeed);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
