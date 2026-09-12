using System;
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

    // 费用左下角的元素角标。元素 SVG 原生就是 64x64，必须用槽位强制缩小，
    // 否则会和费用球一样大。这个值应明显小于 64。
    private const float PlayIconSize = 22f;
    private const float PlayNudgeX = -8f; // 负数往左，正数往右
    private const float PlayNudgeY = -8f; // 负数往上，贴在费用球左上角外沿

    // 卡牌正上方的幼苗组。幼苗必须明显大于需求元素。
    private const float SproutSize = 34f;
    private const float ReqIconSize = 20f;
    private const float StackGap = 4f;
    private const float AboveCardGap = 0f; // 整组底边到卡顶的空隙，0 贴住卡顶
    private const float StackOffsetX = 0f; // 正数整组右移
    private const float StackOffsetY = 0f; // 正数整组下移

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

        int reqCount = seed.Requirements.Sum(req => req.Value);
        float reqWidth = reqCount * ReqIconSize + Math.Max(0, reqCount - 1) * StackGap;
        float stackWidth = Math.Max(SproutSize, reqWidth);
        float stackHeight = StackHeight;

        Control stack = new()
        {
            Name = SeedStackName,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            CustomMinimumSize = new Vector2(stackWidth, stackHeight)
        };
        ForceBox(stack, stackWidth, stackHeight);

        Control sprout = MakeIcon("res://botanist/images/elements/sprout.svg", SproutSize, "BotanistSprout");
        sprout.Position = new Vector2((stackWidth - SproutSize) / 2f, 0f);
        stack.AddChild(sprout);

        float reqX = (stackWidth - reqWidth) / 2f;
        foreach (var requirement in seed.Requirements)
        {
            for (int i = 0; i < requirement.Value; i++)
            {
                Control icon = MakeIcon(requirement.Key.IconPath(), ReqIconSize);
                icon.Position = new Vector2(reqX, SproutSize + StackGap);
                stack.AddChild(icon);
                reqX += ReqIconSize + StackGap;
            }
        }

        stack.Position = SeedStackPosition(stackWidth);
        host.AddChild(stack);
    }

    private static float StackHeight => SproutSize + StackGap + ReqIconSize;

    private static Vector2 SeedStackPosition(float stackWidth)
    {
        // 以卡面中心为原点：X=0 是正中，Y 负半高是卡顶。
        return new Vector2(
            -stackWidth * 0.5f + StackOffsetX,
            -NCard.defaultSize.Y * 0.5f - StackHeight - AboveCardGap + StackOffsetY);
    }

    private static Control MakeIcon(string path, float size, string? name = null)
    {
        Control slot = new()
        {
            Name = name ?? "BotanistIcon",
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        ForceSize(slot, size);

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

    private static void ForceSize(Control node, float size)
    {
        ForceBox(node, size, size);
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
