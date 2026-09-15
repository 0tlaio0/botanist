using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace Botanist.Scripts;

public class BotanistSlowReleaseFertilizerPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string? CustomPackedIconPath => BotanistArt.ElementIcon(BotanistElement.Earth);
    public override string? CustomBigIconPath => BotanistArt.ElementIcon(BotanistElement.Earth);
}
