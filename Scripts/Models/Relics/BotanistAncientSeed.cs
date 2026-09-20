using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Botanist.Scripts;

/// <summary>每场战斗开始时从随机种子中选择一张免费加入手牌。</summary>
[Pool(typeof(BotanistRelicPool))]
public class BotanistAncientSeed : CustomRelicModel
{
    private const int SeedChoiceCount = 3;

    public override RelicRarity Rarity => RelicRarity.Rare;

    public override string PackedIconPath => BotanistArt.Character;
    protected override string PackedIconOutlinePath => BotanistArt.Character;
    protected override string BigIconPath => BotanistArt.Character;

    public override async Task BeforeCombatStart()
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
            new BlockingPlayerChoiceContext(),
            seedChoices,
            Owner,
            canSkip: true);
        if (chosenSeed == null)
        {
            return;
        }

        chosenSeed.SetToFreeThisTurn();
        Flash();
        await CardPileCmd.AddGeneratedCardToCombat(chosenSeed, PileType.Hand, Owner);
    }
}
