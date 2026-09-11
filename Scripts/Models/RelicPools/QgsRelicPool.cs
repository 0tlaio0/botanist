using BaseLib.Abstracts;

namespace Qgs.Scripts;

/// <summary>亓官夙的专属遗物池。</summary>
public class QgsRelicPool : CustomRelicPoolModel
{
    public override string? TextEnergyIconPath => "res://qgs/images/energy.svg";
    public override string? BigEnergyIconPath => "res://qgs/images/energy_big.svg";
}
