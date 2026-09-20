using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using MegaCrit.Sts2.Core.Runs;

namespace Botanist.Scripts;

public sealed partial class BotanistGraftScreen : Control, IOverlayScreen
{
    private enum GraftScreenAction
    {
        Cancel = 0,
        SelectRoot = 1,
        SelectScion = 2,
        Confirm = 3
    }

    private sealed class SlotView
    {
        public required VBoxContainer Container { get; init; }
        public required Button Button { get; init; }
        public required CenterContainer Preview { get; init; }
        public required Label Hint { get; init; }
    }

    private readonly SlotView _rootSlot;
    private readonly SlotView _resultSlot;
    private readonly SlotView _scionSlot;
    private readonly RichTextLabel _resultSummary;
    private readonly Label _statusLabel;
    private readonly Button _confirmButton;
    private readonly Button _cancelButton;

    private TaskCompletionSource<GraftScreenAction>? _pendingAction;
    private CardModel? _root;
    private CardModel? _scion;
    private CardModel? _preview;
    private bool _closed;

    public NetScreenType ScreenType => NetScreenType.CardSelection;
    public bool UseSharedBackstop => true;
    public Control? DefaultFocusedControl => _rootSlot.Button;

    private BotanistGraftScreen(Player owner)
    {
        Name = "BotanistGraftScreen";
        MouseFilter = MouseFilterEnum.Stop;
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);

        ColorRect backstop = new()
        {
            Color = new Color(0.015f, 0.025f, 0.022f, 0.94f),
            MouseFilter = MouseFilterEnum.Stop
        };
        AddChild(backstop);
        backstop.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);

        MarginContainer margin = new();
        margin.AddThemeConstantOverride("margin_left", 64);
        margin.AddThemeConstantOverride("margin_top", 42);
        margin.AddThemeConstantOverride("margin_right", 64);
        margin.AddThemeConstantOverride("margin_bottom", 42);
        AddChild(margin);
        margin.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);

        VBoxContainer layout = new()
        {
            Alignment = BoxContainer.AlignmentMode.Center
        };
        layout.AddThemeConstantOverride("separation", 16);
        margin.AddChild(layout);

        Label title = new()
        {
            Text = Text("GRAFT_SCREEN.title"),
            HorizontalAlignment = HorizontalAlignment.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        title.AddThemeFontSizeOverride("font_size", 36);
        layout.AddChild(title);

        HBoxContainer cards = new()
        {
            Alignment = BoxContainer.AlignmentMode.Center,
            SizeFlagsVertical = SizeFlags.ExpandFill
        };
        cards.AddThemeConstantOverride("separation", 28);
        layout.AddChild(cards);

        _rootSlot = CreateSlot(
            "GRAFT_SCREEN.root",
            "GRAFT_SCREEN.rootHint",
            GraftScreenAction.SelectRoot,
            () => CompleteAction(GraftScreenAction.SelectRoot));
        _resultSlot = CreateSlot(
            "GRAFT_SCREEN.result",
            "GRAFT_SCREEN.resultEmpty",
            null,
            null);
        _scionSlot = CreateSlot(
            "GRAFT_SCREEN.scion",
            "GRAFT_SCREEN.scionHint",
            GraftScreenAction.SelectScion,
            () => CompleteAction(GraftScreenAction.SelectScion));
        cards.AddChild(_rootSlot.Container);
        cards.AddChild(_resultSlot.Container);
        cards.AddChild(_scionSlot.Container);

        _resultSummary = new RichTextLabel
        {
            BbcodeEnabled = true,
            Text = Text("GRAFT_SCREEN.resultEmpty"),
            CustomMinimumSize = new Vector2(0f, 52f),
            HorizontalAlignment = HorizontalAlignment.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        _resultSummary.AddThemeFontSizeOverride("normal_font_size", 16);
        layout.AddChild(_resultSummary);

        HBoxContainer footer = new()
        {
            Alignment = BoxContainer.AlignmentMode.Center
        };
        footer.AddThemeConstantOverride("separation", 20);
        layout.AddChild(footer);

        _statusLabel = new Label
        {
            Text = Text("GRAFT_SCREEN.ready"),
            VerticalAlignment = VerticalAlignment.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        footer.AddChild(_statusLabel);

        _cancelButton = CreateFooterButton("GRAFT_SCREEN.cancel");
        _cancelButton.Pressed += () => CompleteAction(GraftScreenAction.Cancel);
        footer.AddChild(_cancelButton);

        _confirmButton = CreateFooterButton("GRAFT_SCREEN.confirm");
        _confirmButton.Pressed += () => CompleteAction(GraftScreenAction.Confirm);
        footer.AddChild(_confirmButton);

        SetSelection(null, null, null);
    }

    public static async Task<bool> RunAsync(Player owner)
    {
        if (RunManager.Instance?.PlayerChoiceSynchronizer is not { } synchronizer)
        {
            return false;
        }

        bool isLocal = ShouldHandleLocalChoice(owner);
        NOverlayStack? overlayStack = NOverlayStack.Instance;
        BotanistGraftScreen? screen = null;
        if (isLocal && overlayStack != null)
        {
            try
            {
                screen = new BotanistGraftScreen(owner);
                overlayStack.Push(screen);
            }
            catch (Exception ex)
            {
                Log.Warn($"Botanist graft screen failed to open: {ex}");
                screen?.QueueFreeSafely();
                screen = null;
            }
        }

        try
        {
            CardModel? root = null;
            CardModel? scion = null;
            CardModel? preview = null;

            while (true)
            {
                // 联机两端必须按相同顺序预留同一个选择编号，才能让取消和确认保持一致。
                GraftScreenAction action = await WaitForActionAsync(
                    owner,
                    screen,
                    synchronizer,
                    isLocal);
                switch (action)
                {
                    case GraftScreenAction.Cancel:
                        return false;

                    case GraftScreenAction.SelectRoot:
                        CardModel? selectedRoot = await SelectRootAsync(owner);
                        if (selectedRoot != null)
                        {
                            root = selectedRoot;
                            scion = null;
                            preview = null;
                            screen?.SetSelection(root, scion, preview);
                        }

                        break;

                    case GraftScreenAction.SelectScion:
                        if (root == null)
                        {
                            break;
                        }

                        CardModel? selectedScion = await SelectScionAsync(owner, root);
                        if (selectedScion != null)
                        {
                            scion = selectedScion;
                            preview = BotanistGraftService.CreatePreviewCard(root, scion);
                            screen?.SetSelection(root, scion, preview);
                        }

                        break;

                    case GraftScreenAction.Confirm:
                        if (root == null ||
                            scion == null ||
                            preview == null ||
                            !BotanistGraftService.CanGraft(root, scion))
                        {
                            break;
                        }

                        return await BotanistGraftService.ApplyGraftAsync(root, scion);
                }
            }
        }
        catch (Exception ex)
        {
            Log.Warn($"Botanist graft screen failed: {ex}");
            return false;
        }
        finally
        {
            if (screen != null &&
                GodotObject.IsInstanceValid(screen) &&
                !screen._closed &&
                overlayStack != null)
            {
                overlayStack.Remove(screen);
            }
        }
    }

    public override void _UnhandledInput(InputEvent inputEvent)
    {
        if (!Visible || _closed || !inputEvent.IsActionPressed("ui_cancel"))
        {
            return;
        }

        GetViewport().SetInputAsHandled();
        CompleteAction(GraftScreenAction.Cancel);
    }

    public void AfterOverlayOpened()
    {
        Modulate = Colors.Transparent;
        Tween fade = CreateTween();
        fade.TweenProperty(this, "modulate:a", 1f, 0.18)
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Cubic);
    }

    public void AfterOverlayClosed()
    {
        _closed = true;
        _pendingAction?.TrySetResult(GraftScreenAction.Cancel);
        _pendingAction = null;
        this.QueueFreeSafely();
    }

    public void AfterOverlayShown()
    {
        Visible = true;
        RefreshButtonStates();
    }

    public void AfterOverlayHidden()
    {
        Visible = false;
        SetButtonsEnabled(false);
    }

    private static async Task<GraftScreenAction> WaitForActionAsync(
        Player owner,
        BotanistGraftScreen? screen,
        PlayerChoiceSynchronizer synchronizer,
        bool isLocal)
    {
        uint choiceId = synchronizer.ReserveChoiceId(owner);
        if (!isLocal)
        {
            int index = (await synchronizer.WaitForRemoteChoice(owner, choiceId)).AsIndex();
            return index switch
            {
                (int)GraftScreenAction.SelectRoot => GraftScreenAction.SelectRoot,
                (int)GraftScreenAction.SelectScion => GraftScreenAction.SelectScion,
                (int)GraftScreenAction.Confirm => GraftScreenAction.Confirm,
                _ => GraftScreenAction.Cancel
            };
        }

        if (screen == null)
        {
            synchronizer.SyncLocalChoice(
                owner,
                choiceId,
                PlayerChoiceResult.FromIndex((int)GraftScreenAction.Cancel));
            return GraftScreenAction.Cancel;
        }

        GraftScreenAction action = GraftScreenAction.Cancel;
        try
        {
            action = await screen.WaitForActionAsync();
            return action;
        }
        finally
        {
            synchronizer.SyncLocalChoice(
                owner,
                choiceId,
                PlayerChoiceResult.FromIndex((int)action));
        }
    }

    private static async Task<CardModel?> SelectRootAsync(Player owner)
    {
        CardSelectorPrefs prefs = new(
            new LocString("rest_site_ui", "GRAFT_SCREEN.rootPrompt"),
            1)
        {
            Cancelable = true,
            RequireManualConfirmation = true
        };
        IEnumerable<CardModel> selected = await CardSelectCmd.FromDeckGeneric(
            owner,
            prefs,
            BotanistGraftService.CanBeGraftRoot);
        return selected.FirstOrDefault();
    }

    private static async Task<CardModel?> SelectScionAsync(
        Player owner,
        CardModel root)
    {
        CardSelectorPrefs prefs = new(
            new LocString("rest_site_ui", "GRAFT_SCREEN.scionPrompt"),
            1)
        {
            Cancelable = true,
            RequireManualConfirmation = true
        };
        IEnumerable<CardModel> selected = await CardSelectCmd.FromDeckGeneric(
            owner,
            prefs,
            card => BotanistGraftService.CanBeScion(card) &&
                    BotanistGraftService.CanGraft(root, card));
        return selected.FirstOrDefault();
    }

    private static SlotView CreateSlot(
        string titleKey,
        string hintKey,
        GraftScreenAction? action,
        Action? onClick)
    {
        VBoxContainer container = new()
        {
            CustomMinimumSize = new Vector2(320f, 500f),
            Alignment = BoxContainer.AlignmentMode.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill
        };
        container.AddThemeConstantOverride("separation", 10);

        Label title = new()
        {
            Text = Text(titleKey),
            HorizontalAlignment = HorizontalAlignment.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        title.AddThemeFontSizeOverride("font_size", 24);
        container.AddChild(title);

        PanelContainer panel = new()
        {
            CustomMinimumSize = new Vector2(300f, 380f),
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill
        };
        StyleBoxFlat panelStyle = new()
        {
            BgColor = new Color(0.055f, 0.09f, 0.075f, 0.98f),
            BorderColor = new Color(0.48f, 0.63f, 0.36f, 0.85f),
            BorderWidthLeft = 2,
            BorderWidthTop = 2,
            BorderWidthRight = 2,
            BorderWidthBottom = 2,
            CornerRadiusTopLeft = 10,
            CornerRadiusTopRight = 10,
            CornerRadiusBottomLeft = 10,
            CornerRadiusBottomRight = 10
        };
        panel.AddThemeStyleboxOverride("panel", panelStyle);
        container.AddChild(panel);

        CenterContainer preview = new()
        {
            CustomMinimumSize = new Vector2(270f, 350f),
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill
        };
        panel.AddChild(preview);

        Button button = new()
        {
            Flat = true,
            FocusMode = FocusModeEnum.All,
            MouseDefaultCursorShape = CursorShape.PointingHand,
            MouseFilter = MouseFilterEnum.Stop,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill,
            Disabled = action == null
        };
        panel.AddChild(button);
        if (onClick != null)
        {
            button.Pressed += onClick;
        }

        Label hint = new()
        {
            Text = Text(hintKey),
            HorizontalAlignment = HorizontalAlignment.Center,
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        hint.AddThemeFontSizeOverride("font_size", 15);
        container.AddChild(hint);

        return new SlotView
        {
            Container = container,
            Button = button,
            Preview = preview,
            Hint = hint
        };
    }

    private static Button CreateFooterButton(string key)
    {
        Button button = new()
        {
            Text = Text(key),
            CustomMinimumSize = new Vector2(170f, 52f),
            FocusMode = FocusModeEnum.All
        };
        button.AddThemeFontSizeOverride("font_size", 20);
        return button;
    }

    private async Task<GraftScreenAction> WaitForActionAsync()
    {
        _pendingAction = new TaskCompletionSource<GraftScreenAction>();
        RefreshButtonStates();
        GraftScreenAction action = await _pendingAction.Task;
        SetButtonsEnabled(false);
        return action;
    }

    private void CompleteAction(GraftScreenAction action)
    {
        TaskCompletionSource<GraftScreenAction>? pending = _pendingAction;
        if (pending == null || _closed)
        {
            return;
        }

        _pendingAction = null;
        SetButtonsEnabled(false);
        pending.TrySetResult(action);
    }

    private void SetSelection(
        CardModel? root,
        CardModel? scion,
        CardModel? preview)
    {
        _root = root;
        _scion = scion;
        _preview = preview;
        PopulateSlot(_rootSlot, root, "GRAFT_SCREEN.rootHint");
        PopulateSlot(_scionSlot, scion, "GRAFT_SCREEN.scionHint");
        PopulateSlot(_resultSlot, preview, "GRAFT_SCREEN.resultEmpty");
        _resultSummary.Text = preview == null
            ? Text("GRAFT_SCREEN.resultEmpty")
            : BotanistGraftService.BuildRipenSummary(preview);
        RefreshButtonStates();
    }

    private void PopulateSlot(SlotView slot, CardModel? card, string emptyKey)
    {
        foreach (Node child in slot.Preview.GetChildren())
        {
            child.QueueFreeSafely();
        }

        slot.Hint.Text = card?.Title ?? Text(emptyKey);
        if (card == null)
        {
            return;
        }

        NCard? cardNode = NCard.Create(card);
        NGridCardHolder? holder = cardNode == null
            ? null
            : NGridCardHolder.Create(cardNode);
        if (holder == null)
        {
            cardNode?.QueueFreeSafely();
            return;
        }

        slot.Preview.AddChild(holder);
        holder.Scale = Vector2.One * 0.92f;
        cardNode!.UpdateVisuals(PileType.Deck, CardPreviewMode.Normal);
    }

    private void RefreshButtonStates()
    {
        bool waiting = _pendingAction != null && !_closed;
        _rootSlot.Button.Disabled = !waiting;
        _scionSlot.Button.Disabled = !waiting ||
                                    _root == null ||
                                    !HasEligibleScion(_root);
        _confirmButton.Disabled = !waiting ||
                                  _root == null ||
                                  _scion == null ||
                                  _preview == null ||
                                  !BotanistGraftService.CanGraft(_root, _scion);
        _cancelButton.Disabled = !waiting;
        _statusLabel.Text = _confirmButton.Disabled
            ? Text("GRAFT_SCREEN.resultEmpty")
            : Text("GRAFT_SCREEN.ready");
    }

    private static bool HasEligibleScion(CardModel root)
    {
        return PileType.Deck.GetPile(root.Owner).Cards.Any(
            card => BotanistGraftService.CanBeScion(card) &&
                    BotanistGraftService.CanGraft(root, card));
    }

    private void SetButtonsEnabled(bool enabled)
    {
        _rootSlot.Button.Disabled = !enabled;
        _scionSlot.Button.Disabled = !enabled;
        _confirmButton.Disabled = !enabled;
        _cancelButton.Disabled = !enabled;
    }

    private static string Text(string key)
    {
        return new LocString("rest_site_ui", key).GetFormattedText();
    }

    private static bool ShouldHandleLocalChoice(Player owner)
    {
        return LocalContext.IsMe(owner) &&
               RunManager.Instance?.NetService.Type != NetGameType.Replay;
    }
}
