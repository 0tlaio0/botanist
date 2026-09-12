using BaseLib.Abstracts;

namespace Botanist.Scripts;

/// <summary>植物学家的专属药水池。</summary>
public class BotanistPotionPool : CustomPotionPoolModel
{
    public override string? TextEnergyIconPath => BotanistArt.Energy;
    public override string? BigEnergyIconPath => BotanistArt.BigEnergy;
}
