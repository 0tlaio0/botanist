using BaseLib.Abstracts;

namespace Botanist.Scripts;

/// <summary>植物学家的专属遗物池。</summary>
public class BotanistRelicPool : CustomRelicPoolModel
{
    public override string? TextEnergyIconPath => BotanistArt.Energy;
    public override string? BigEnergyIconPath => BotanistArt.BigEnergy;
}
