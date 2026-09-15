using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Botanist.Scripts;

/// <summary>本回合使用的牌与上一张牌元素不同时抽牌。</summary>
public class BotanistCropRotationPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string? CustomPackedIconPath => BotanistArt.ElementIcon(BotanistElement.Wind);
    public override string? CustomBigIconPath => BotanistArt.ElementIcon(BotanistElement.Wind);

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner.Player ||
            cardPlay.Card is BotanistCropRotation ||
            cardPlay.Card is not BotanistCardModel currentCard ||
            currentCard.Element == BotanistElement.None)
        {
            return;
        }

        CardPlayFinishedEntry? previousEntry = CombatManager.Instance.History.CardPlaysFinished
            .LastOrDefault(entry =>
                entry.CardPlay != cardPlay &&
                entry.CardPlay.Card.Owner == Owner.Player &&
                entry.HappenedThisTurn(CombatState));

        if (previousEntry?.CardPlay.Card is not BotanistCardModel previousCard ||
            previousCard.Element == BotanistElement.None ||
            previousCard.Element == currentCard.Element)
        {
            return;
        }

        Flash();
        await CardPileCmd.Draw(choiceContext, Amount, Owner.Player);
    }
}
