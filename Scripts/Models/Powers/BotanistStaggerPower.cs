using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Botanist.Scripts;

/// <summary>本回合降低力量，并在敌方回合结束时恢复。</summary>
public class BotanistStaggerPower : TemporaryStrengthPower
{
    public override AbstractModel OriginModel => ModelDb.Card<BotanistVineStaggerer>();
    protected override bool IsPositive => false;

    public override LocString Title =>
        new("powers", "BOTANIST-BOTANIST_STAGGER_POWER.title");

    public override LocString Description =>
        new("powers", "BOTANIST-BOTANIST_STAGGER_POWER.description");

    protected override string SmartDescriptionLocKey =>
        "BOTANIST-BOTANIST_STAGGER_POWER.smartDescription";
}
