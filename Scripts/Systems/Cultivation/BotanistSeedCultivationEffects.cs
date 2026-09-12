using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Botanist.Scripts;

/// <summary>统一处理种子成熟后的能力、遗物和衍生牌结算。</summary>
internal static class BotanistSeedCultivationEffects
{
    public static async Task TriggerAsync(
        PlayerChoiceContext choiceContext,
        Player player,
        PlantedSeed seed)
    {
        if (player.Creature.GetPower<BotanistSeedBankPower>() is { } seedBank)
        {
            await seedBank.OnSeedCultivated(choiceContext, seed.Card);
        }

        if (player.Creature.GetPower<BotanistSpecimenCasePower>() is { } specimenCase)
        {
            for (int i = 0; i < specimenCase.Amount; i++)
            {
                await AddSpecimenToDraw(player, seed.Seed);
            }
        }

        if (player.GetRelic<BotanistHeavenPalmBottle>() is { } bottle)
        {
            await bottle.OnSeedCultivated(choiceContext);
        }
    }

    private static async Task AddSpecimenToDraw(Player player, IBotanistSeedCard seed)
    {
        ICombatState? combatState = player.Creature.CombatState;
        if (combatState == null)
        {
            return;
        }

        BotanistSpecimen specimen = combatState.CreateCard<BotanistSpecimen>(player);
        specimen.SetCopiedSeed(seed);
        await CardPileCmd.AddGeneratedCardToCombat(
            specimen,
            PileType.Draw,
            player,
            CardPilePosition.Random);
    }
}
