using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Botanist.Scripts;

/// <summary>接下来打出的攻击牌生成同元素插条。</summary>
public class BotanistSerialCuttingPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string? CustomPackedIconPath => BotanistArt.ElementIcon(BotanistElement.Wind);
    public override string? CustomBigIconPath => BotanistArt.ElementIcon(BotanistElement.Wind);

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner.Player ||
            cardPlay.Card.Type != CardType.Attack ||
            cardPlay.Card.IsCutting() ||
            Amount <= 0 ||
            !cardPlay.IsLastInSeries)
        {
            return;
        }

        BotanistElement element = cardPlay.Card is BotanistCardModel botanistCard
            ? botanistCard.Element
            : BotanistElement.None;
        Flash();
        await BotanistCuttings.CreateInHand(choiceContext, Owner.Player, element);
        await PowerCmd.Decrement(this);
    }
}
