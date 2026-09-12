using BaseLib.Abstracts;

namespace Botanist.Scripts;

/// <summary>植物学家的专属药水池。</summary>
public class BotanistPotionPool : CustomPotionPoolModel
{
    public override string? TextEnergyIconPath => "res://botanist/images/energy.svg";
    public override string? BigEnergyIconPath => "res://botanist/images/energy_big.svg";
}
