using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Botanist.Scripts;

/// <summary>种子成长时，随机推进其后一颗种子的元素需求。</summary>
public class BotanistFloweringSynchronizationPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string? CustomPackedIconPath => BotanistArt.ElementIcon(BotanistElement.Wind);
    public override string? CustomBigIconPath => BotanistArt.ElementIcon(BotanistElement.Wind);

    public async Task OnSeedCultivated(
        PlayerChoiceContext choiceContext,
        PlantedSeed nextSeed)
    {
        if (Owner.Player is not { } player)
        {
            return;
        }

        Flash();
        await BotanistCultivation.ReduceRandomRequirementOfSeed(
            choiceContext,
            player,
            nextSeed,
            Amount);
    }
}
