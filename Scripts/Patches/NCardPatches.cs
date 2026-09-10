using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace Qgs.Scripts;

[HarmonyPatch(typeof(NCard), nameof(NCard.UpdateVisuals))]
public static class NCardChromePatch
{
    public static void Postfix(NCard __instance)
    {
        QgsCardChrome.Refresh(__instance);
    }
}

[HarmonyPatch(typeof(NCard), nameof(NCard.OnReturnedFromPool))]
public static class NCardChromePoolPatch
{
    public static void Postfix(NCard __instance)
    {
        QgsCardChrome.Clear(__instance);
    }
}

[HarmonyPatch(typeof(NCard), "UpdateTypePlaque")]
public static class NCardSeedTypePatch
{
    public static void Postfix(NCard __instance)
    {
        if (__instance.Model is not QgsSeedCardModel)
        {
            return;
        }

        MegaCrit.Sts2.addons.mega_text.MegaLabel typeLabel = __instance.GetNode<MegaCrit.Sts2.addons.mega_text.MegaLabel>("%TypeLabel");
        typeLabel.SetTextAutoSize("种子");
    }
}

[HarmonyPatch(typeof(NCombatUi), nameof(NCombatUi._Ready))]
public static class NCombatUiCultivationPatch
{
    public static void Postfix()
    {
        NQgsCultivationZone.TryAttach();
    }
}
