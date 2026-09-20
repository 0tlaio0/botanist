using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Relics;

namespace Botanist.Scripts;

/// <summary>每场战斗的第一颗已种下种子，其所有成长需求减少一点。</summary>
[Pool(typeof(BotanistRelicPool))]
public class BotanistBoneMeal : CustomRelicModel
{
    private bool _hasTriggeredThisCombat;

    public override RelicRarity Rarity => RelicRarity.Shop;

    public override string PackedIconPath => BotanistArt.Character;
    protected override string PackedIconOutlinePath => BotanistArt.Character;
    protected override string BigIconPath => BotanistArt.Character;

    public override Task BeforeCombatStart()
    {
        _hasTriggeredThisCombat = false;
        return Task.CompletedTask;
    }

    public bool TryApplyToFirstPlantedSeed()
    {
        if (_hasTriggeredThisCombat)
        {
            return false;
        }

        _hasTriggeredThisCombat = true;
        Flash();
        return true;
    }
}
