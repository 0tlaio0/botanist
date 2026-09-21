using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Botanist.Scripts;

public static class BotanistCuttings
{
    public static bool IsCutting(this CardModel card) =>
        card.Tags.Contains(BotanistCardTags.Cutting);

    public static Task<BotanistCutting?> CreateInHand(
        PlayerChoiceContext choiceContext,
        Player owner,
        BotanistElement element,
        bool forked = false)
    {
        return CreateOne(choiceContext, owner, PileType.Hand, CardPilePosition.Bottom, element, forked);
    }

    public static Task<BotanistCutting?> CreateOnDrawTop(
        PlayerChoiceContext choiceContext,
        Player owner,
        BotanistElement element,
        bool forked = false)
    {
        return CreateOne(choiceContext, owner, PileType.Draw, CardPilePosition.Top, element, forked);
    }

    public static Task<BotanistCutting?> CreateOnDrawBottom(
        PlayerChoiceContext choiceContext,
        Player owner,
        BotanistElement element,
        bool forked = false)
    {
        return CreateOne(choiceContext, owner, PileType.Draw, CardPilePosition.Bottom, element, forked);
    }

    public static Task<BotanistCutting?> CreateInDiscard(
        PlayerChoiceContext choiceContext,
        Player owner,
        BotanistElement element,
        bool forked = false)
    {
        return CreateOne(choiceContext, owner, PileType.Discard, CardPilePosition.Bottom, element, forked);
    }

    private static async Task<BotanistCutting?> CreateOne(
        PlayerChoiceContext choiceContext,
        Player owner,
        PileType pile,
        CardPilePosition position,
        BotanistElement element,
        bool forked)
    {
        if (owner.Creature.CombatState is not { } combatState)
        {
            return null;
        }

        BotanistCutting cutting = combatState.CreateCard<BotanistCutting>(owner);
        cutting.SetElement(element);
        cutting.ForksToLowest = forked;
        await CardPileCmd.AddGeneratedCardToCombat(cutting, pile, owner, position);
        if (owner.Creature.GetPower<BotanistSharpenPower>() is { } sharpen)
        {
            await sharpen.OnCuttingGenerated(choiceContext);
        }

        return cutting;
    }
}
