using BaseLib.Abstracts;
using Godot;

namespace Qgs.Scripts;

/// <summary>亓官夙的专属卡牌池。</summary>
public class QgsCardPool : CustomCardPoolModel
{
    public override string Title => "qgs";
    public override string? TextEnergyIconPath => "res://qgs/images/energy.svg";
    public override string? BigEnergyIconPath => "res://qgs/images/energy_big.svg";
    public override Color DeckEntryCardColor => new(0.36f, 0.62f, 0.78f);
    public override Color ShaderColor => new(0.36f, 0.62f, 0.78f);
    public override bool IsColorless => false;
}
