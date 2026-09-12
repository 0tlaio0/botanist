using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Rooms;

namespace Botanist.Scripts;

/// <summary>在战斗结束时，按累计成熟次数回复生命。</summary>
public class BotanistStarFruitPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string? CustomPackedIconPath => BotanistArt.ElementIcon(BotanistElement.Water);
    public override string? CustomBigIconPath => BotanistArt.ElementIcon(BotanistElement.Water);

    public override async Task AfterCombatEnd(CombatRoom room)
    {
        if (Owner.IsDead)
        {
            return;
        }

        Flash();
        await CreatureCmd.Heal(Owner, Amount);
    }
}
