using System.Collections.Generic;
using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.HoverTips;

namespace Botanist.Scripts;

public partial class NBotanistCultivationOrb : NClickableControl
{
    private const float OrbSize = 64f;
    private const float OrbLeft = 38f;
    private const float EmptyOrbSize = 51f;

    private PlantedSeed? _planted;
    private Panel _orbShell = null!;
    private Panel _orbFill = null!;
    private Panel _highlight = null!;
    private TextureRect _emptyVisual = null!;
    private VBoxContainer _content = null!;
    private Label _progressLabel = null!;
    private Label _titleLabel = null!;

    public PlantedSeed? Planted => _planted;

    public void Initialize(PlantedSeed? planted)
    {
        _planted = planted;
    }

    public void SetPlanted(PlantedSeed? planted)
    {
        _planted = planted;
        if (IsNodeReady())
        {
            RefreshVisual();
        }
    }

    public override void _Ready()
    {
        ConnectSignals();

        CustomMinimumSize = new Vector2(140f, 108f);
        Size = CustomMinimumSize;
        FocusMode = FocusModeEnum.All;
        MouseFilter = MouseFilterEnum.Stop;
        MouseDefaultCursorShape = CursorShape.PointingHand;

        BuildVisuals();
        RefreshVisual();
    }

    protected override void OnFocus()
    {
        if (_planted == null)
        {
            return;
        }

        LocString title = new("cards", $"{_planted.Card.Id.Entry}.title");
        HoverTip hoverTip = new(title, BuildHoverDescription(), PrimaryIcon());
        NHoverTipSet.CreateAndShow(this, hoverTip, HoverTip.GetHoverTipAlignment(this))?.SetFollowOwner();
    }

    protected override void OnUnfocus()
    {
        NHoverTipSet.Remove(this);
    }

    private void BuildVisuals()
    {
        _orbShell = new Panel
        {
            Position = new Vector2(OrbLeft, 0f),
            Size = new Vector2(OrbSize, OrbSize),
            MouseFilter = MouseFilterEnum.Ignore
        };
        _orbShell.AddThemeStyleboxOverride("panel", CreateOrbStyle(
            new Color(0.12f, 0.18f, 0.16f, 0.92f),
            new Color(0.78f, 0.88f, 0.82f, 0.72f),
            2f));
        AddChild(_orbShell);

        _orbFill = new Panel
        {
            Position = new Vector2(OrbLeft + 5f, 5f),
            Size = new Vector2(OrbSize - 10f, OrbSize - 10f),
            MouseFilter = MouseFilterEnum.Ignore
        };
        AddChild(_orbFill);

        _highlight = new Panel
        {
            Position = new Vector2(OrbLeft + 13f, 10f),
            Size = new Vector2(25f, 16f),
            MouseFilter = MouseFilterEnum.Ignore
        };
        _highlight.AddThemeStyleboxOverride("panel", CreateOrbStyle(
            new Color(1f, 1f, 1f, 0.14f),
            Colors.Transparent,
            0f));
        _highlight.Rotation = -0.35f;
        AddChild(_highlight);

        _emptyVisual = new TextureRect
        {
            Texture = BotanistArt.Load("res://images/orbs/empty_orb.png"),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            CustomMinimumSize = new Vector2(EmptyOrbSize, EmptyOrbSize),
            Size = new Vector2(EmptyOrbSize, EmptyOrbSize),
            Position = new Vector2(
                OrbLeft + (OrbSize - EmptyOrbSize) / 2f,
                (OrbSize - EmptyOrbSize) / 2f),
            Modulate = Colors.White,
            MouseFilter = MouseFilterEnum.Ignore
        };
        AddChild(_emptyVisual);

        _content = new VBoxContainer
        {
            Position = new Vector2(OrbLeft + 5f, 12f),
            Size = new Vector2(OrbSize - 10f, 42f),
            Alignment = BoxContainer.AlignmentMode.Center,
            MouseFilter = MouseFilterEnum.Ignore
        };
        _content.AddThemeConstantOverride("separation", 1);
        AddChild(_content);

        _progressLabel = CreateLabel(string.Empty, 20, new Color(1f, 0.98f, 0.9f));
        _progressLabel.AddThemeColorOverride("font_outline_color", new Color(0.08f, 0.11f, 0.1f, 0.95f));
        _progressLabel.AddThemeConstantOverride("outline_size", 7);
        _content.AddChild(_progressLabel);

        _titleLabel = CreateLabel(string.Empty, 14, new Color(0.92f, 0.97f, 0.93f));
        _titleLabel.Position = new Vector2(0f, 70f);
        _titleLabel.Size = new Vector2(140f, 34f);
        _titleLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        _titleLabel.MaxLinesVisible = 2;
        _titleLabel.TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis;
        _titleLabel.AddThemeColorOverride("font_outline_color", new Color(0.04f, 0.07f, 0.05f, 0.95f));
        _titleLabel.AddThemeConstantOverride("outline_size", 6);
        AddChild(_titleLabel);
    }

    private void RefreshVisual()
    {
        if (_planted == null)
        {
            _orbShell.Visible = false;
            _orbFill.Visible = false;
            _highlight.Visible = false;
            _emptyVisual.Visible = true;
            _content.Visible = false;
            _progressLabel.Visible = false;
            _titleLabel.Visible = false;
            return;
        }

        Color primary = ElementColor(_planted.Seed.Requirements[0].Key);
        _orbShell.AddThemeStyleboxOverride("panel", CreateOrbStyle(
            new Color(0.08f, 0.12f, 0.11f, 0.96f),
            primary.Lightened(0.32f),
            2f));
        _orbFill.AddThemeStyleboxOverride("panel", CreateOrbStyle(
            new Color(primary.R, primary.G, primary.B, 0.72f),
            primary.Lightened(0.22f),
            1f));
        _orbShell.Visible = true;
        _orbFill.Visible = true;
        _highlight.Visible = true;
        _emptyVisual.Visible = false;
        _content.Visible = true;

        BuildElementIcons();

        int remaining = _planted.Remaining.Values.Sum();
        int required = _planted.Seed.Requirements.Sum(requirement => requirement.Value);
        _progressLabel.Text = $"{remaining}/{required}";
        _progressLabel.Visible = true;
        _titleLabel.Text = _planted.Card.Title;
        _titleLabel.Visible = true;
    }

    private void BuildElementIcons()
    {
        foreach (Node child in _content.GetChildren())
        {
            if (child is HBoxContainer)
            {
                _content.RemoveChild(child);
                child.QueueFree();
            }
        }

        HBoxContainer row = new()
        {
            Alignment = BoxContainer.AlignmentMode.Center,
            MouseFilter = MouseFilterEnum.Ignore
        };
        row.AddThemeConstantOverride("separation", 2);

        foreach (KeyValuePair<BotanistElement, int> requirement in _planted!.Seed.Requirements)
        {
            Texture2D? texture = BotanistArt.Load(requirement.Key.IconPath());
            if (texture == null)
            {
                continue;
            }

            TextureRect icon = new()
            {
                Texture = texture,
                CustomMinimumSize = new Vector2(18f, 18f),
                ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
                StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
                MouseFilter = MouseFilterEnum.Ignore
            };
            int remaining = _planted.Remaining.GetValueOrDefault(requirement.Key);
            icon.Modulate = remaining > 0
                ? Colors.White
                : new Color(0.55f, 0.62f, 0.58f, 0.5f);
            row.AddChild(icon);
        }

        _content.AddChild(row);
        _content.MoveChild(row, 0);
    }

    private string BuildHoverDescription()
    {
        PlantedSeed planted = _planted!;
        List<string> lines = [];

        foreach (KeyValuePair<BotanistElement, int> requirement in planted.Seed.Requirements)
        {
            int remaining = planted.Remaining.GetValueOrDefault(requirement.Key);
            lines.Add($"{requirement.Key.DisplayName()}：{remaining}/{requirement.Value}");
        }

        lines.Add(string.Empty);
        lines.Add($"成熟时：{planted.Seed.RipenSummary}");
        return string.Join("\n", lines);
    }

    private Texture2D? PrimaryIcon()
    {
        return _planted == null
            ? null
            : BotanistArt.Load(_planted.Seed.Requirements[0].Key.IconPath());
    }

    private static Label CreateLabel(string text, int size, Color color)
    {
        Label label = new()
        {
            Text = text,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            MouseFilter = MouseFilterEnum.Ignore
        };
        label.AddThemeColorOverride("font_color", color);
        label.AddThemeFontSizeOverride("font_size", size);
        return label;
    }

    private static StyleBoxFlat CreateOrbStyle(Color background, Color border, float borderWidth)
    {
        const int radius = 32;
        return new StyleBoxFlat
        {
            BgColor = background,
            BorderColor = border,
            BorderWidthLeft = (int)borderWidth,
            BorderWidthTop = (int)borderWidth,
            BorderWidthRight = (int)borderWidth,
            BorderWidthBottom = (int)borderWidth,
            CornerRadiusTopLeft = radius,
            CornerRadiusTopRight = radius,
            CornerRadiusBottomRight = radius,
            CornerRadiusBottomLeft = radius
        };
    }

    private static Color ElementColor(BotanistElement element)
    {
        return element switch
        {
            BotanistElement.Earth => new Color(0.48f, 0.7f, 0.3f),
            BotanistElement.Fire => new Color(0.92f, 0.36f, 0.18f),
            BotanistElement.Water => new Color(0.24f, 0.58f, 0.88f),
            BotanistElement.Wind => new Color(0.42f, 0.82f, 0.68f),
            BotanistElement.Aether => new Color(0.95f, 0.78f, 0.3f),
            _ => new Color(0.55f, 0.68f, 0.6f)
        };
    }
}
