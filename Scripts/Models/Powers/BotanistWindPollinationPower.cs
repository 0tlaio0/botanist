using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Botanist.Scripts;

/// <summary>每回合限制并结算前若干张风元素牌的抽牌效果。</summary>
public class BotanistWindPollinationPower : CustomPowerModel
{
    private readonly Dictionary<CardModel, int> _activeAmounts = [];
    private int _windCardsPlayedThisTurn;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string? CustomPackedIconPath => BotanistArt.ElementIcon(BotanistElement.Wind);
    public override string? CustomBigIconPath => BotanistArt.ElementIcon(BotanistElement.Wind);

    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner == Owner.Player && IsWindCard(cardPlay.Card))
        {
            _activeAmounts[cardPlay.Card] = Amount;
        }

        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner.Player ||
            !_activeAmounts.Remove(cardPlay.Card, out int activeAmount) ||
            activeAmount <= 0 ||
            _windCardsPlayedThisTurn >= activeAmount)
        {
            return;
        }

        _windCardsPlayedThisTurn++;
        Flash();
        await CardPileCmd.Draw(choiceContext, 1, Owner.Player);
    }

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player == Owner.Player)
        {
            _windCardsPlayedThisTurn = 0;
            _activeAmounts.Clear();
        }

        return Task.CompletedTask;
    }

    private static bool IsWindCard(CardModel card) =>
        card is BotanistCardModel { Element: BotanistElement.Wind };
}
