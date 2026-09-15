using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Botanist.Scripts;

public class BotanistAutoDripPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string? CustomPackedIconPath => BotanistArt.ElementIcon(BotanistElement.Water);
    public override string? CustomBigIconPath => BotanistArt.ElementIcon(BotanistElement.Water);

    public override async Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext,
        MegaCrit.Sts2.Core.Entities.Players.Player player)
    {
        if (player != Owner.Player)
        {
            return;
        }

        Flash();
        for (int i = 0; i < Amount; i++)
        {
            await BotanistCultivation.AbsorbElement(
                choiceContext,
                player,
                BotanistElement.Water);
        }
    }
}
