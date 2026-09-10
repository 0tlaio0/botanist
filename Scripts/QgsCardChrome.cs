using System;
using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.Entities.UI;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace Qgs.Scripts;

public static class QgsCardChrome
{
    private const string PlayElementName = "QgsPlayElement";
    private const string SeedStackName = "QgsSeedStack";
    private const float PlayIconSize = 26f;
    private const float SproutSize = 32f;
    private const float ReqIconSize = 20f;

    public static void Refresh(NCard card)
    {
        Clear(card);

        if (card.Model is not QgsCardModel qgsCard || card.Visibility != ModelVisibility.Visible)
        {
            return;
        }

        card.ClipContents = false;
        Control host = card.Body ?? card;

        if (qgsCard.Element != QgsElement.None)
        {
            TextureRect playIcon = MakeIcon(qgsCard.Element.IconPath(), PlayIconSize);
            playIcon.Name = PlayElementName;
            playIcon.Position = new Vector2(22f, 28f);
            playIcon.ZIndex = 6;
            host.AddChild(playIcon);
        }

        if (qgsCard is QgsSeedCardModel seed)
        {
            AttachSeedStack(card, seed);
        }
    }

    public static void Clear(NCard card)
    {
        Control host = card.Body ?? card;
        host.GetNodeOrNull(PlayElementName)?.Free();
        card.GetNodeOrNull(SeedStackName)?.Free();
        host.GetNodeOrNull(SeedStackName)?.Free();
    }

    private static void AttachSeedStack(NCard card, QgsSeedCardModel seed)
    {
        VBoxContainer stack = new()
        {
            Name = SeedStackName,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            Alignment = BoxContainer.AlignmentMode.Center
        };
        stack.AddThemeConstantOverride("separation", 3);
        stack.ZIndex = 12;

        TextureRect sprout = MakeIcon("res://qgs/images/elements/sprout.svg", SproutSize);
        sprout.Name = "QgsSprout";
        stack.AddChild(sprout);

        HBoxContainer requirements = new()
        {
            Name = "QgsGrowthElements",
            MouseFilter = Control.MouseFilterEnum.Ignore,
            Alignment = BoxContainer.AlignmentMode.Center
        };
        requirements.AddThemeConstantOverride("separation", 3);

        foreach (var requirement in seed.Requirements)
        {
            for (int i = 0; i < requirement.Value; i++)
            {
                requirements.AddChild(MakeIcon(requirement.Key.IconPath(), ReqIconSize));
            }
        }

        stack.AddChild(requirements);

        int reqCount = seed.Requirements.Sum(req => req.Value);
        float stackWidth = Math.Max(SproutSize, reqCount * (ReqIconSize + 3f));
        float stackHeight = SproutSize + 3f + ReqIconSize;
        stack.Size = new Vector2(stackWidth, stackHeight);
        float cardWidth = card.Size.X > 1f ? card.Size.X : 300f;
        stack.Position = new Vector2(cardWidth / 2f - stackWidth / 2f, -40f);
        card.AddChild(stack);
    }

    private static TextureRect MakeIcon(string path, float size)
    {
        return new TextureRect
        {
            Texture = QgsArt.Load(path),
            CustomMinimumSize = new Vector2(size, size),
            Size = new Vector2(size, size),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
    }
}
