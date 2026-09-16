using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Botanist.Scripts;

/// <summary>种子成长时抽取对应数量的牌。</summary>
public class BotanistSlashAndBurnPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string? CustomPackedIconPath => BotanistArt.ElementIcon(BotanistElement.Fire);
    public override string? CustomBigIconPath => BotanistArt.ElementIcon(BotanistElement.Fire);

    public async Task OnSeedCultivated(PlayerChoiceContext choiceContext)
    {
        if (Owner.Player is not { } player)
        {
            return;
        }

        Flash();
        await CardPileCmd.Draw(choiceContext, Amount, player);
    }
}
