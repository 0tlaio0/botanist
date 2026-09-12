using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace Botanist.Scripts;

[HarmonyPatch(typeof(NCard), nameof(NCard.UpdateVisuals))]
public static class NCardChromePatch
{
    public static void Postfix(NCard __instance)
    {
        BotanistCardChrome.Refresh(__instance);
    }
}

[HarmonyPatch(typeof(NCard), nameof(NCard.OnReturnedFromPool))]
public static class NCardChromePoolPatch
{
    public static void Postfix(NCard __instance)
    {
        BotanistCardChrome.Clear(__instance);
    }
}

[HarmonyPatch(typeof(NCombatUi), nameof(NCombatUi._Ready))]
public static class NCombatUiCultivationPatch
{
    public static void Postfix()
    {
        NBotanistCultivationZone.TryAttach();
    }
}
