using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Botanist.Scripts;

/// <summary>植物成熟时对所有可攻击敌人施加中毒。</summary>
public class BotanistAllelopathyPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string? CustomPackedIconPath => BotanistArt.ElementIcon(BotanistElement.Fire);
    public override string? CustomBigIconPath => BotanistArt.ElementIcon(BotanistElement.Fire);

    public async Task OnPlantMatured(PlayerChoiceContext choiceContext)
    {
        if (CombatState is not { } combatState || combatState.HittableEnemies.Count == 0)
        {
            return;
        }

        Flash();
        await PowerCmd.Apply<PoisonPower>(
            choiceContext,
            combatState.HittableEnemies,
            Amount,
            Owner,
            null);
    }
}
