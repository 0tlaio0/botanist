using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace Qgs.Scripts;

public partial class NQgsCultivationZone : HBoxContainer
{
    private const string NodeName = "QgsCultivationZone";

    public override void _Ready()
    {
        Name = NodeName;
        MouseFilter = MouseFilterEnum.Ignore;
        Alignment = AlignmentMode.Center;
        AddThemeConstantOverride("separation", 12);
        SetAnchorsPreset(LayoutPreset.CenterTop);
        OffsetLeft = -330;
        OffsetRight = 330;
        OffsetTop = 18;
        OffsetBottom = 168;
        GrowHorizontal = GrowDirection.Both;
        ZIndex = 40;

        if (QgsCultivation.Instance != null)
        {
            QgsCultivation.Instance.Changed += Refresh;
        }

        Refresh();
    }

    public override void _ExitTree()
    {
        if (QgsCultivation.Instance != null)
        {
            QgsCultivation.Instance.Changed -= Refresh;
        }
    }

    public static void TryAttach()
    {
        NCombatUi? ui = NCombatRoom.Instance?.Ui;
        if (ui == null || ui.GetNodeOrNull(NodeName) != null)
        {
            return;
        }

        CombatState? combatState = CombatManager.Instance.DebugOnlyGetState();
        Player? player = LocalContext.GetMe(combatState);
        if (player?.Character is not QgsCharacter)
        {
            return;
        }

        ui.AddChild(new NQgsCultivationZone());
    }

    public static void Detach()
    {
        Node? zone = NCombatRoom.Instance?.Ui?.GetNodeOrNull(NodeName);
        zone?.QueueFree();
    }

    private void Refresh()
    {
        foreach (Node child in GetChildren())
        {
            child.QueueFree();
        }

        CombatState? combatState = CombatManager.Instance.DebugOnlyGetState();
        Player? player = LocalContext.GetMe(combatState);
        IReadOnlyList<PlantedSeed> planted = player == null
            ? []
            : QgsCultivation.GetPlanted(player);

        for (int i = 0; i < QgsCultivation.Capacity; i++)
        {
            AddChild(CreateSlot(i < planted.Count ? planted[i] : null));
        }
    }

    private static Control CreateSlot(PlantedSeed? planted)
    {
        PanelContainer slot = new()
        {
            CustomMinimumSize = new Vector2(200, 132),
            MouseFilter = MouseFilterEnum.Ignore
        };
        slot.AddThemeStyleboxOverride("panel", CreateSlotStyle(planted != null));

        VBoxContainer contents = new()
        {
            MouseFilter = MouseFilterEnum.Ignore
        };
        contents.AddThemeConstantOverride("separation", 4);
        slot.AddChild(contents);

        if (planted == null)
        {
            contents.AddChild(CreateLabel("空位", 18, new Color(0.75f, 0.85f, 0.8f, 0.7f)));
            contents.AddChild(CreateLabel("培养区", 14, new Color(0.65f, 0.75f, 0.7f, 0.6f)));
            return slot;
        }

        contents.AddChild(CreateLabel(planted.Card.Title, 20, new Color(0.93f, 0.97f, 0.9f)));
        contents.AddChild(CreateRequirementRow(planted));
        contents.AddChild(CreateLabel("成熟时：" + planted.Card.RipenSummary, 14, new Color(1f, 0.86f, 0.45f)));
        return slot;
    }

    private static Control CreateRequirementRow(PlantedSeed planted)
    {
        HBoxContainer row = new()
        {
            Alignment = AlignmentMode.Center,
            MouseFilter = MouseFilterEnum.Ignore
        };
        row.AddThemeConstantOverride("separation", 8);

        foreach (KeyValuePair<QgsElement, int> requirement in planted.Card.Requirements)
        {
            HBoxContainer pair = new()
            {
                MouseFilter = MouseFilterEnum.Ignore
            };
            pair.AddThemeConstantOverride("separation", 2);

            Texture2D? texture = QgsArt.Load(requirement.Key.IconPath());
            if (texture != null)
            {
                pair.AddChild(new TextureRect
                {
                    Texture = texture,
                    CustomMinimumSize = new Vector2(22, 22),
                    ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
                    StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
                    MouseFilter = MouseFilterEnum.Ignore
                });
            }

            int remaining = planted.Remaining.GetValueOrDefault(requirement.Key);
            pair.AddChild(CreateLabel($"{remaining}/{requirement.Value}{requirement.Key.DisplayName()}", 16, new Color(0.95f, 0.93f, 0.78f)));
            row.AddChild(pair);
        }

        return row;
    }

    private static Label CreateLabel(string text, int size, Color color)
    {
        Label label = new()
        {
            Text = text,
            HorizontalAlignment = HorizontalAlignment.Center,
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            MouseFilter = MouseFilterEnum.Ignore
        };
        label.AddThemeColorOverride("font_color", color);
        label.AddThemeFontSizeOverride("font_size", size);
        return label;
    }

    private static StyleBoxFlat CreateSlotStyle(bool filled)
    {
        return new StyleBoxFlat
        {
            BgColor = filled ? new Color(0.09f, 0.16f, 0.12f, 0.88f) : new Color(0.07f, 0.1f, 0.09f, 0.55f),
            BorderColor = filled ? new Color(0.72f, 0.86f, 0.45f, 0.95f) : new Color(0.45f, 0.55f, 0.48f, 0.55f),
            BorderWidthLeft = 2,
            BorderWidthTop = 2,
            BorderWidthRight = 2,
            BorderWidthBottom = 2,
            CornerRadiusTopLeft = 10,
            CornerRadiusTopRight = 10,
            CornerRadiusBottomRight = 10,
            CornerRadiusBottomLeft = 10,
            ContentMarginLeft = 10,
            ContentMarginTop = 10,
            ContentMarginRight = 10,
            ContentMarginBottom = 10
        };
    }
}
