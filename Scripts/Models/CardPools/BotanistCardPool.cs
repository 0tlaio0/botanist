using BaseLib.Abstracts;
using Godot;

namespace Botanist.Scripts;

/// <summary>植物学家的专属卡牌池。</summary>
public class BotanistCardPool : CustomCardPoolModel
{
    public override string Title => "botanist";
    public override string? TextEnergyIconPath => "res://botanist/images/energy.svg";
    public override string? BigEnergyIconPath => "res://botanist/images/energy_big.svg";
    public override Color DeckEntryCardColor => new(0.36f, 0.62f, 0.78f);
    public override Color ShaderColor => new(0.36f, 0.62f, 0.78f);
    public override bool IsColorless => false;
}
