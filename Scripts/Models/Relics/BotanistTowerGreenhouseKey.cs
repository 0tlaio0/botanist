using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Botanist.Scripts;

/// <summary>每场战斗开始时增加培育区容量。</summary>
[Pool(typeof(BotanistRelicPool))]
public class BotanistTowerGreenhouseKey : CustomRelicModel
{
    private const int CapacityBonus = 2;

    public override RelicRarity Rarity => RelicRarity.Uncommon;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new IntVar("Capacity", CapacityBonus)];

    public override string PackedIconPath => BotanistArt.Character;
    protected override string PackedIconOutlinePath => BotanistArt.Character;
    protected override string BigIconPath => BotanistArt.Character;

    public override Task BeforeCombatStart()
    {
        BotanistCultivation.AddCapacity(Owner, DynamicVars["Capacity"].IntValue);
        Flash();
        return Task.CompletedTask;
    }
}
