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

/// <summary>每回合首次打出火元素牌时提供能量。</summary>
public class BotanistIncubatorPower : CustomPowerModel
{
    private readonly Dictionary<CardModel, int> _activeAmounts = [];
    private bool _triggeredThisTurn;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string? CustomPackedIconPath => BotanistArt.ElementIcon(BotanistElement.Fire);
    public override string? CustomBigIconPath => BotanistArt.ElementIcon(BotanistElement.Fire);

    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner == Owner.Player && IsFireCard(cardPlay.Card))
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
            _triggeredThisTurn)
        {
            return;
        }

        _triggeredThisTurn = true;
        Flash();
        await PlayerCmd.GainEnergy(activeAmount, Owner.Player);
    }

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player == Owner.Player)
        {
            _triggeredThisTurn = false;
            _activeAmounts.Clear();
        }

        return Task.CompletedTask;
    }

    private static bool IsFireCard(CardModel card) =>
        card is BotanistCardModel { Element: BotanistElement.Fire };
}
