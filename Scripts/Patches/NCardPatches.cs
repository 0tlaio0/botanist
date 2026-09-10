using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace Qgs.Scripts;

[HarmonyPatch(typeof(NCard), "UpdateEnergyCostVisuals")]
public static class NCardElementIconPatch
{
    private const string IconNodeName = "QgsElementIcons";

    public static void Postfix(NCard __instance, TextureRect ___energyIcon)
    {
        Node? existing = ___energyIcon.GetNodeOrNull(IconNodeName);
        existing?.Free();

        if (__instance.Model is not QgsCardModel qgsCard || qgsCard.Element == QgsElement.None)
        {
            return;
        }

        HBoxContainer row = new()
        {
            Name = IconNodeName,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            Alignment = BoxContainer.AlignmentMode.Center
        };
        row.AddThemeConstantOverride("separation", 2);
        row.Position = new Vector2(4, 58);
        row.Size = new Vector2(56, 24);

        foreach (QgsElement element in qgsCard.Element.IconElements())
        {
            Texture2D? texture = QgsArt.Load(element.IconPath());
            if (texture == null)
            {
                continue;
            }

            row.AddChild(new TextureRect
            {
                Texture = texture,
                CustomMinimumSize = new Vector2(22, 22),
                ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
                StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
                MouseFilter = Control.MouseFilterEnum.Ignore
            });
        }

        ___energyIcon.AddChild(row);
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
