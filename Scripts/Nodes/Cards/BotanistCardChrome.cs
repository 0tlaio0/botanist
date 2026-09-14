using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.Entities.UI;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace Botanist.Scripts;

public static class BotanistCardChrome
{
    private const string PlayElementName = "BotanistPlayElement";
    private const string SeedStackName = "BotanistSeedStack";

    // ---------------------------------------------------------------------------
    // 手调：改这些常量后执行 `dotnet build`，再重启游戏。
    // 坐标：X 向右为正，Y 向下为正，单位是卡面像素。能量球按 64x64 计。
    // ---------------------------------------------------------------------------

    // 费用球左上角的元素角标。元素图必须用槽位强制缩小，
    // 否则会和费用球一样大。这个值应明显小于 64。
    private const float PlayIconSize = 42f;
    private const float PlayNudgeX = -10f; // 负数往左，正数往右
    private const float PlayNudgeY = -14f; // 负数往上，向费用球中心靠近

    // 卡牌正上方的种子幼苗。需求元素覆盖在三根主根末端。
    private const float SeedlingWidth = 252f;
    private const float SeedlingHeight = 139f;
    private const float ReqIconSize = 36f;
    private const float ReqIconTop = SeedlingHeight - ReqIconSize * 0.82f;
    private const float SeedlingDepth = 14f;
    private const float CountLabelHeight = 18f;
    private const float AboveCardGap = 0f; // 整组底边到卡顶的空隙，0 贴住卡顶
    private const float StackOffsetX = -14f; // 负数整组左移
    private const float StackOffsetY = 18f; // 正数整组下移，让根须压入卡牌上边缘

    // card.tscn 里 EnergyIcon 相对卡面中心的左上角。
    private static readonly Vector2 EnergyOrbTopLeft = new(-166f, -227f);

    public static void Refresh(NCard card)
    {
        Clear(card);

        if (card.Model is not BotanistCardModel botanistCard || card.Visibility != ModelVisibility.Visible)
        {
            return;
        }

        card.ClipContents = false;
        if (card.Body != null)
        {
            card.Body.ClipContents = false;
        }

        if (botanistCard.Element != BotanistElement.None)
        {
            AttachPlayElement(card, botanistCard);
        }

        if (botanistCard.AsSeed() is { } seed)
        {
            AttachSeedStack(card, seed);
        }
    }

    public static void Clear(NCard card)
    {
        card.GetNodeOrNull("%EnergyIcon")?.GetNodeOrNull(PlayElementName)?.Free();
        card.GetNodeOrNull(PlayElementName)?.Free();
        card.Body?.GetNodeOrNull(PlayElementName)?.Free();
        card.GetNodeOrNull(SeedStackName)?.Free();
        card.Body?.GetNodeOrNull(SeedStackName)?.Free();
    }

    private static void AttachPlayElement(NCard card, BotanistCardModel botanistCard)
    {
        Control playIcon = MakeIcon(botanistCard.Element.IconPath(), PlayIconSize, PlayElementName);
        TextureRect? energyIcon = card.GetNodeOrNull<TextureRect>("%EnergyIcon");

        // 不要抬 ZIndex。尺寸必须先定好，再写 Position；ForceBox 若再清 Offset，
        // 挂到 NCard 时会回到卡心，看起来就像贴在类型牌上。
        if (energyIcon != null)
        {
            energyIcon.ClipContents = false;
            energyIcon.AddChild(playIcon);
            playIcon.Position = PlayElementOffset();
            return;
        }

        card.AddChild(playIcon);
        playIcon.Position = EnergyOrbTopLeft + PlayElementOffset();
    }

    private static Vector2 PlayElementOffset()
    {
        return new Vector2(PlayNudgeX, PlayNudgeY);
    }

    private static void AttachSeedStack(NCard card, IBotanistSeedCard seed)
    {
        // 挂到 NCard 本身：它的原点在卡面中心。挂到 Body 时 TitleBanner 的本地 X
        // 不在同一空间，整组会贴到卡面左侧。
        Control host = card;
        host.ClipContents = false;

        bool growthFree = BotanistCultivation.IsGrowthFree(seed.Card);
        List<KeyValuePair<BotanistElement, int>> requirements = seed.Requirements
            .Where(requirement => requirement.Value > 0)
            .ToList();
        bool showCountLabels = growthFree || requirements.Any(requirement => requirement.Value > 1);
        int reqCount = requirements.Count;
        float stackWidth = SeedlingWidth;
        float stackHeight = StackHeight();

        Control stack = new()
        {
            Name = SeedStackName,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            CustomMinimumSize = new Vector2(stackWidth, stackHeight)
        };
        ForceBox(stack, stackWidth, stackHeight);

        Control seedling = MakeIcon(
            BotanistArt.SeedlingSeed,
            SeedlingWidth,
            SeedlingHeight,
            "BotanistSeedling");
        seedling.Position = new Vector2(0f, SeedlingDepth);
        stack.AddChild(seedling);

        IReadOnlyList<float> anchorCenters = RequirementAnchorCenters(reqCount);
        for (int index = 0; index < requirements.Count; index++)
        {
            KeyValuePair<BotanistElement, int> requirement = requirements[index];
            Control icon = MakeIcon(requirement.Key.IconPath(), ReqIconSize);
            float reqX = anchorCenters[index] - ReqIconSize * 0.5f;
            icon.Position = new Vector2(reqX, ReqIconTop);
            stack.AddChild(icon);

            int displayedCount = growthFree ? 0 : requirement.Value;
            if (growthFree || displayedCount > 1)
            {
                Label countLabel = new()
                {
                    Text = displayedCount.ToString(),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    MouseFilter = Control.MouseFilterEnum.Ignore
                };
                countLabel.AddThemeFontSizeOverride("font_size", 16);
                countLabel.AddThemeColorOverride("font_color", Colors.White);
                countLabel.AddThemeColorOverride("font_outline_color", Colors.Black);
                countLabel.AddThemeConstantOverride("outline_size", 4);
                ForceBox(countLabel, ReqIconSize, CountLabelHeight);
                countLabel.Position = new Vector2(
                    reqX,
                    ReqIconTop + ReqIconSize - CountLabelHeight * 0.5f);
                stack.AddChild(countLabel);
            }
        }

        stack.Position = SeedStackPosition(stackWidth, stackHeight);
        host.AddChild(stack);
    }

    private static float StackHeight()
    {
        return ReqIconTop + ReqIconSize;
    }

    private static IReadOnlyList<float> RequirementAnchorCenters(int requirementCount)
    {
        return requirementCount switch
        {
            <= 1 => [SeedlingWidth * 0.54f],
            2 => [SeedlingWidth * 0.27f, SeedlingWidth * 0.81f],
            _ => [SeedlingWidth * 0.27f, SeedlingWidth * 0.54f, SeedlingWidth * 0.81f]
        };
    }

    private static Vector2 SeedStackPosition(float stackWidth, float stackHeight)
    {
        // 以卡面中心为原点：X=0 是正中，Y 负半高是卡顶。
        return new Vector2(
            -stackWidth * 0.5f + StackOffsetX,
            -NCard.defaultSize.Y * 0.5f - stackHeight - AboveCardGap + StackOffsetY);
    }

    private static Control MakeIcon(string path, float size, string? name = null)
    {
        return MakeIcon(path, size, size, name);
    }

    private static Control MakeIcon(string path, float width, float height, string? name = null)
    {
        Control slot = new()
        {
            Name = name ?? "BotanistIcon",
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        ForceBox(slot, width, height);

        TextureRect icon = new()
        {
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        icon.Texture = BotanistArt.Load(path);
        icon.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        icon.OffsetLeft = 0;
        icon.OffsetTop = 0;
        icon.OffsetRight = 0;
        icon.OffsetBottom = 0;
        slot.AddChild(icon);
        return slot;
    }

    private static void ForceBox(Control node, float width, float height)
    {
        Vector2 pos = node.Position;
        node.CustomMinimumSize = new Vector2(width, height);
        node.SetAnchorsPreset(Control.LayoutPreset.TopLeft);
        node.Size = new Vector2(width, height);
        node.Position = pos;
    }
}
