using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Botanist.Scripts;

/// <summary>打出高耗能牌后按层数吸取火元素。</summary>
public class BotanistGreenhouseEffectPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string? CustomPackedIconPath => BotanistArt.ElementIcon(BotanistElement.Fire);
    public override string? CustomBigIconPath => BotanistArt.ElementIcon(BotanistElement.Fire);

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner.Player ||
            cardPlay.Card is BotanistGreenhouseEffect ||
            cardPlay.Resources.EnergyValue < 2)
        {
            return;
        }

        Flash();
        for (int i = 0; i < Amount; i++)
        {
            await BotanistCultivation.AbsorbElement(
                choiceContext,
                Owner.Player,
                BotanistElement.Fire);
        }
    }
}
