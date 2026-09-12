using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Rooms;

namespace Botanist.Scripts;

public partial class NBotanistCultivationZone : Control
{
    private const string NodeName = "BotanistCultivationZone";
    private const float OrbWidth = 140f;
    private const float LayoutDuration = 0.45f;
    private static readonly Vector2 ZoneCenter = new(260f, 160f);

    private readonly Dictionary<PlantedSeed, NBotanistCultivationOrb> _seedOrbs = [];
    private readonly Dictionary<PlantedSeed, int> _seedIndices = [];
    private readonly Dictionary<NBotanistCultivationOrb, int> _emptyOrbs = [];

    private static NBotanistCultivationZone? _active;
    private static bool _attachPending;

    private Player? _player;
    private NCreature? _creatureNode;
    private Tween? _layoutTween;
    private bool _flipArc;
    private int _layoutCapacity;

    public void Initialize(Player player)
    {
        _player = player;
    }

    public override void _Ready()
    {
        Name = NodeName;
        MouseFilter = MouseFilterEnum.Ignore;
        SetAnchorsPreset(LayoutPreset.TopLeft);
        Size = new Vector2(520f, 320f);
        PivotOffset = ZoneCenter;
        ZIndex = 0;
        ProcessMode = ProcessModeEnum.Always;
        Visible = CombatManager.Instance.IsInProgress && NCombatRoom.Instance?.Ui?.Visible == true;

        if (BotanistCultivation.Instance != null)
        {
            BotanistCultivation.Instance.Changed += Refresh;
        }

        Refresh();
    }

    public override void _EnterTree()
    {
        base._EnterTree();
        CombatManager.Instance.CombatEnded += OnCombatEnded;
        CombatManager.Instance.CombatWon += OnCombatWon;
    }

    public override void _Process(double delta)
    {
        if (!CombatManager.Instance.IsInProgress || NCombatRoom.Instance?.Ui?.Visible != true)
        {
            Visible = false;
            return;
        }

        Visible = true;

        if (_player == null)
        {
            return;
        }

        bool flipArc = (_player.PlayerCombatState?.OrbQueue.Capacity ?? 0) > 0;
        if (flipArc != _flipArc)
        {
            _flipArc = flipArc;
            Refresh();
        }

        _creatureNode = NCombatRoom.Instance?.GetCreatureNode(_player.Creature);
        if (_creatureNode == null || !IsInstanceValid(_creatureNode))
        {
            return;
        }

        float creatureScale = _creatureNode.Visuals.Scale.X;
        float visualScale = creatureScale > 1f
            ? 1f
            : Mathf.Lerp(creatureScale, 1f, 0.5f);
        Vector2 separationOffset = _flipArc
            ? Vector2.Up * 120f * visualScale
            : Vector2.Zero;
        Scale = Vector2.One * visualScale;
        GlobalPosition = _creatureNode.Visuals.OrbPosition.GlobalPosition
            - ZoneCenter * visualScale
            + separationOffset;
    }

    public override void _ExitTree()
    {
        CombatManager.Instance.CombatEnded -= OnCombatEnded;
        CombatManager.Instance.CombatWon -= OnCombatWon;

        if (_active == this)
        {
            _active = null;
        }

        if (BotanistCultivation.Instance != null)
        {
            BotanistCultivation.Instance.Changed -= Refresh;
        }
    }

    public static void TryAttach()
    {
        NCombatRoom? room = NCombatRoom.Instance;
        if (room == null || _attachPending || (_active != null && IsInstanceValid(_active)))
        {
            return;
        }

        CombatState? combatState = CombatManager.Instance.DebugOnlyGetState();
        Player? player = LocalContext.GetMe(combatState);
        if (player?.Character is not BotanistCharacter)
        {
            return;
        }

        NCreature? creature = room.GetCreatureNode(player.Creature);
        if (creature == null)
        {
            _attachPending = true;
            Callable.From(DeferredAttach).CallDeferred();
            return;
        }

        if (creature.GetNodeOrNull(NodeName) != null)
        {
            return;
        }

        NBotanistCultivationZone zone = new();
        zone.Initialize(player);
        _active = zone;
        creature.AddChild(zone);
    }

    public static void Detach()
    {
        _active?.QueueFree();
        _active = null;
        _attachPending = false;
    }

    private static void DeferredAttach()
    {
        _attachPending = false;
        TryAttach();
    }

    private static void OnCombatEnded(CombatRoom room)
    {
        Detach();
    }

    private static void OnCombatWon(CombatRoom room)
    {
        Detach();
    }

    private void Refresh()
    {
        if (_player == null)
        {
            return;
        }

        int capacity = BotanistCultivation.GetCapacity(_player);
        int previousCapacity = _layoutCapacity == 0 ? capacity : _layoutCapacity;
        IReadOnlyList<PlantedSeed> planted = BotanistCultivation.GetPlanted(_player);

        _layoutTween?.Kill();
        _layoutTween = CreateTween().SetParallel();

        SyncSeedOrbs(planted, previousCapacity, capacity);
        SyncEmptyOrbs(planted.Count, capacity);
        _layoutCapacity = capacity;
    }

    private void SyncSeedOrbs(
        IReadOnlyList<PlantedSeed> planted,
        int previousCapacity,
        int capacity)
    {
        HashSet<PlantedSeed> activeSeeds = [.. planted];
        foreach (KeyValuePair<PlantedSeed, NBotanistCultivationOrb> entry in new List<KeyValuePair<PlantedSeed, NBotanistCultivationOrb>>(_seedOrbs))
        {
            if (activeSeeds.Contains(entry.Key))
            {
                continue;
            }

            _seedOrbs.Remove(entry.Key);
            _seedIndices.Remove(entry.Key);
            AnimateOut(entry.Value);
        }

        for (int seedIndex = 0; seedIndex < planted.Count; seedIndex++)
        {
            PlantedSeed seed = planted[seedIndex];
            if (!_seedOrbs.TryGetValue(seed, out NBotanistCultivationOrb? orb))
            {
                orb = CreateSeedOrb(seed, SlotPosition(seedIndex, capacity));
                _seedOrbs[seed] = orb;
                _seedIndices[seed] = seedIndex;
                continue;
            }

            int previousIndex = _seedIndices.GetValueOrDefault(seed, seedIndex);
            orb.SetPlanted(seed);
            AnimateSeedAlongArc(orb, previousIndex, previousCapacity, seedIndex, capacity);
            _seedIndices[seed] = seedIndex;
        }
    }

    private void SyncEmptyOrbs(int seedCount, int capacity)
    {
        HashSet<int> desiredIndices = [];
        for (int index = seedCount; index < capacity; index++)
        {
            desiredIndices.Add(index);
        }

        foreach (KeyValuePair<NBotanistCultivationOrb, int> entry in new List<KeyValuePair<NBotanistCultivationOrb, int>>(_emptyOrbs))
        {
            if (desiredIndices.Contains(entry.Value))
            {
                continue;
            }

            _emptyOrbs.Remove(entry.Key);
            AnimateOut(entry.Key);
        }

        for (int index = seedCount; index < capacity; index++)
        {
            NBotanistCultivationOrb? orb = null;
            foreach (KeyValuePair<NBotanistCultivationOrb, int> entry in _emptyOrbs)
            {
                if (entry.Value == index)
                {
                    orb = entry.Key;
                    break;
                }
            }

            if (orb == null)
            {
                orb = CreateEmptyOrb();
                _emptyOrbs[orb] = index;
            }

            orb.SetPlanted(null);
            TweenToSlot(orb, SlotPosition(index, capacity));
        }
    }

    private NBotanistCultivationOrb CreateEmptyOrb()
    {
        NBotanistCultivationOrb orb = new();
        orb.Initialize(null);
        orb.Position = EmptySlotOrigin();
        AddChild(orb);
        return orb;
    }

    private NBotanistCultivationOrb CreateSeedOrb(PlantedSeed planted, Vector2 target)
    {
        NBotanistCultivationOrb orb = new();
        orb.Initialize(planted);
        orb.Position = target;
        orb.Scale = Vector2.Zero;
        orb.Modulate = new Color(1f, 1f, 1f, 0f);
        AddChild(orb);

        _layoutTween?.Parallel().TweenProperty(orb, "scale", Vector2.One, 0.3f)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.Out);
        _layoutTween?.Parallel().TweenProperty(orb, "modulate", Colors.White, 0.2f);
        return orb;
    }

    private void AnimateSeedAlongArc(
        NBotanistCultivationOrb orb,
        int previousIndex,
        int previousCapacity,
        int nextIndex,
        int nextCapacity)
    {
        if (previousIndex == nextIndex && previousCapacity == nextCapacity)
        {
            TweenToSlot(orb, SlotPosition(nextIndex, nextCapacity));
            return;
        }

        _layoutTween?.Parallel().TweenMethod(
            Callable.From<float>(t =>
            {
                float index = Mathf.Lerp(previousIndex, nextIndex, t);
                float capacity = Mathf.Lerp(previousCapacity, nextCapacity, t);
                orb.Position = SlotPosition(index, capacity);
            }),
            0f,
            1f,
            LayoutDuration)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);
    }

    private void TweenToSlot(NBotanistCultivationOrb orb, Vector2 target)
    {
        _layoutTween?.Parallel().TweenProperty(orb, "position", target, LayoutDuration)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);
    }

    private void AnimateOut(NBotanistCultivationOrb orb)
    {
        Tween tween = CreateTween().SetParallel();
        tween.TweenProperty(orb, "scale", Vector2.Zero, 0.25f)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.In);
        tween.TweenProperty(orb, "modulate", new Color(1f, 1f, 1f, 0f), 0.2f);
        tween.Chain().TweenCallback(Callable.From(orb.QueueFree));
    }

    private Vector2 SlotPosition(float visualIndex, float capacity)
    {
        float capacitySpread = Mathf.Clamp(
            (capacity - 3f) / (BotanistCultivation.MaxCapacity - 3f),
            0f,
            1f);
        float radius = Mathf.Lerp(225f, 300f, capacitySpread);
        float span = 125f;
        float step = capacity > 1f ? span / (capacity - 1f) : 0f;
        float angle = Mathf.DegToRad(-25f - span + visualIndex * step);
        float verticalDirection = _flipArc ? -1f : 1f;
        Vector2 arcOffset = new(
            -Mathf.Cos(angle) * radius,
            195f + verticalDirection * Mathf.Sin(angle) * radius);
        return ZoneCenter + arcOffset - new Vector2(OrbWidth / 2f, 32f);
    }

    private Vector2 SlotPosition(int visualIndex, int capacity)
    {
        return SlotPosition((float)visualIndex, capacity);
    }

    private static Vector2 EmptySlotOrigin()
    {
        return ZoneCenter + new Vector2(0f, 195f) - new Vector2(OrbWidth / 2f, 32f);
    }
}
