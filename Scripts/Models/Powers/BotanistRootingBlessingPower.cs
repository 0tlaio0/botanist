using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace Botanist.Scripts;

/// <summary>打出种子牌时提供格挡。</summary>
public class BotanistRootingBlessingPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string? CustomPackedIconPath => BotanistArt.ElementIcon(BotanistElement.Earth);
    public override string? CustomBigIconPath => BotanistArt.ElementIcon(BotanistElement.Earth);

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner.Player || !cardPlay.Card.IsSeed())
        {
            return;
        }

        Flash();
        await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Unpowered, null, fast: true);
    }
}
