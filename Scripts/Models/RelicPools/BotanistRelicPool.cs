using BaseLib.Abstracts;

namespace Botanist.Scripts;

/// <summary>植物学家的专属遗物池。</summary>
public class BotanistRelicPool : CustomRelicPoolModel
{
    public override string? TextEnergyIconPath => "res://botanist/images/energy.svg";
    public override string? BigEnergyIconPath => "res://botanist/images/energy_big.svg";
}
