using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace Botanist.Scripts;

/// <summary>为植物学家的初始遗物登记欧洛巴斯之触对应的升级遗物。</summary>
[HarmonyPatch(typeof(TouchOfOrobas), nameof(TouchOfOrobas.GetUpgradedStarterRelic))]
public static class TouchOfOrobasBotanistStarterRelicPatch
{
    [HarmonyPostfix]
    public static void Postfix(RelicModel starterRelic, ref RelicModel __result)
    {
        if (starterRelic.Id == ModelDb.Relic<BotanistStarterRelic>().Id)
        {
            __result = ModelDb.Relic<BotanistWisteriaWreath>();
        }
    }
}
