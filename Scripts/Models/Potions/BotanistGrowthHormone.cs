using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace Botanist.Scripts;

/// <summary>从三张随机种子牌中选一张，本回合免费打出。</summary>
[Pool(typeof(BotanistPotionPool))]
public class BotanistGrowthHormone : CustomPotionModel
{
    private const int SeedChoiceCount = 3;

    public override PotionRarity Rarity => PotionRarity.Uncommon;
    public override PotionUsage Usage => PotionUsage.CombatOnly;
    public override TargetType TargetType => TargetType.Self;
    public override string? CustomPackedImagePath => BotanistArt.Sprout;
    public override string? CustomPackedOutlinePath => BotanistArt.Sprout;

    public override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromKeyword(BotanistKeywords.Growth)];

    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        List<CardModel> seedChoices = CardFactory.GetDistinctForCombat(
                Owner,
                Owner.Character.CardPool
                    .GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint)
                    .Where(card => card.IsSeed()),
                SeedChoiceCount,
                Owner.RunState.Rng.CombatCardGeneration)
            .ToList();

        if (seedChoices.Count == 0)
        {
            return;
        }

        CardModel? chosenSeed = await CardSelectCmd.FromChooseACardScreen(
            choiceContext,
            seedChoices,
            Owner,
            canSkip: true);

        if (chosenSeed == null)
        {
            return;
        }

        chosenSeed.SetToFreeThisTurn();
        BotanistCultivation.MarkGrowthFree(chosenSeed);
        await CardPileCmd.AddGeneratedCardToCombat(chosenSeed, PileType.Hand, Owner);
    }
}
