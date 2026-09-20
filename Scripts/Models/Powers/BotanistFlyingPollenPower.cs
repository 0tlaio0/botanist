using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace Botanist.Scripts;

/// <summary>种子成长时对随机敌人造成伤害。</summary>
public class BotanistFlyingPollenPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string? CustomPackedIconPath => BotanistArt.ElementIcon(BotanistElement.Wind);
    public override string? CustomBigIconPath => BotanistArt.ElementIcon(BotanistElement.Wind);

    public async Task OnSeedCultivated(PlayerChoiceContext choiceContext)
    {
        if (CombatState is not { } combatState || combatState.HittableEnemies.Count == 0)
        {
            return;
        }

        Flash();
        await DamageCmd.Attack(Amount)
            .TargetingRandomOpponents(combatState)
            .Execute(choiceContext);
    }
}
