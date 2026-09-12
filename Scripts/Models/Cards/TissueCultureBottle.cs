// 中文卡名：组培瓶
// 卡面描述：将{Tissues:diff()}张[gold]植物组织[/gold]加入抽牌堆。抽2张[gold]种子[/gold]牌。
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Botanist.Scripts;

[Pool(typeof(BotanistCardPool))]
public class BotanistTissueCultureBottle : BotanistCardModel
{
    private const int SeedDrawCount = 2;

    public override BotanistElement Element => BotanistElement.Earth;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new IntVar("Tissues", 1m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromCard<BotanistPlantTissue>()
    ];

    public BotanistTissueCultureBottle() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await AddPlantTissues(DynamicVars["Tissues"].IntValue);
        await DrawSeedCards(choiceContext, SeedDrawCount);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Tissues"].UpgradeValueBy(1);
    }

    private async Task AddPlantTissues(int count)
    {
        var combatState = CombatState;
        if (combatState == null || count <= 0)
        {
            return;
        }

        IBotanistSeedCard? copiedSeed = BotanistCultivation.GetPlanted(Owner).FirstOrDefault()?.Seed;
        List<CardModel> tissues = [];

        for (int i = 0; i < count; i++)
        {
            BotanistPlantTissue tissue = combatState.CreateCard<BotanistPlantTissue>(Owner);
            tissue.SetCopiedSeed(copiedSeed);
            tissues.Add(tissue);
        }

        await CardPileCmd.AddGeneratedCardsToCombat(
            tissues,
            PileType.Draw,
            Owner,
            CardPilePosition.Random);
    }

    private async Task DrawSeedCards(PlayerChoiceContext choiceContext, int count)
    {
        int handSpace = Math.Max(0, CardPile.MaxCardsInHand - PileType.Hand.GetPile(Owner).Cards.Count);
        int drawsRemaining = Math.Min(count, handSpace);

        while (drawsRemaining > 0)
        {
            await CardPileCmd.ShuffleIfNecessary(choiceContext, Owner);
            CardModel? seed = PileType.Draw.GetPile(Owner).Cards
                .Where(card => card.IsSeed())
                .TakeRandom(1, Owner.RunState.Rng.CombatCardSelection)
                .FirstOrDefault();
            if (seed == null)
            {
                break;
            }

            await CardPileCmd.Add(seed, PileType.Hand);
            drawsRemaining--;
        }
    }
}
