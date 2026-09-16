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

        RecycleInactiveSeedOrbs(planted);
        EnsureOrbCount(capacity);
        BindSeedOrbs(planted, previousCapacity, capacity);
        LayoutEmptyOrbs(planted.Count, previousCapacity, capacity);
        _layoutCapacity = capacity;
    }

    private void RecycleInactiveSeedOrbs(IReadOnlyList<PlantedSeed> planted)
    {
        HashSet<PlantedSeed> activeSeeds = [.. planted];
        foreach (KeyValuePair<PlantedSeed, NBotanistCultivationOrb> entry in new List<KeyValuePair<PlantedSeed, NBotanistCultivationOrb>>(_seedOrbs))
        {
            if (activeSeeds.Contains(entry.Key))
            {
                continue;
            }

            int previousIndex = _seedIndices.Remove(entry.Key, out int index) ? index : -1;
            _seedOrbs.Remove(entry.Key);
            // 回收为空格而不是销毁节点，避免占用切换时少画一格。
            _emptyOrbs[entry.Value] = previousIndex;
        }
    }

    private void EnsureOrbCount(int capacity)
    {
        int orbCount = _seedOrbs.Count + _emptyOrbs.Count;
        for (int index = orbCount; index < capacity; index++)
        {
            NBotanistCultivationOrb orb = CreateEmptyOrb();
            _emptyOrbs[orb] = -1;
        }

        int excess = orbCount - capacity;
        foreach (NBotanistCultivationOrb orb in new List<NBotanistCultivationOrb>(_emptyOrbs.Keys))
        {
            if (excess <= 0)
            {
                break;
            }

            _emptyOrbs.Remove(orb);
            AnimateOut(orb);
            excess--;
        }
    }

    private void BindSeedOrbs(
        IReadOnlyList<PlantedSeed> planted,
        int previousCapacity,
        int capacity)
    {
        for (int seedIndex = 0; seedIndex < planted.Count; seedIndex++)
        {
            PlantedSeed seed = planted[seedIndex];
            if (!_seedOrbs.TryGetValue(seed, out NBotanistCultivationOrb? orb))
            {
                orb = AcquireEmptyOrb(seedIndex, out int newSeedPreviousIndex);
                _seedOrbs[seed] = orb;
                _seedIndices[seed] = seedIndex;
                orb.SetPlanted(seed);
                AnimateOrbAlongArc(orb, newSeedPreviousIndex, previousCapacity, seedIndex, capacity);
                continue;
            }

            int previousIndex = _seedIndices.GetValueOrDefault(seed, seedIndex);
            orb.SetPlanted(seed);
            AnimateOrbAlongArc(orb, previousIndex, previousCapacity, seedIndex, capacity);
            _seedIndices[seed] = seedIndex;
        }
    }

    private NBotanistCultivationOrb AcquireEmptyOrb(int targetIndex, out int previousIndex)
    {
        NBotanistCultivationOrb? bestOrb = null;
        previousIndex = -1;
        int bestDistance = int.MaxValue;

        foreach (KeyValuePair<NBotanistCultivationOrb, int> entry in _emptyOrbs)
        {
            int distance = entry.Value < 0
                ? DistanceFromUnplacedOrb(targetIndex)
                : Math.Abs(entry.Value - targetIndex);
            if (bestOrb != null && distance >= bestDistance)
            {
                continue;
            }

            bestOrb = entry.Key;
            previousIndex = entry.Value;
            bestDistance = distance;
        }

        if (bestOrb != null)
        {
            _emptyOrbs.Remove(bestOrb);
            return bestOrb;
        }

        NBotanistCultivationOrb fallback = CreateEmptyOrb();
        return fallback;
    }

    private static int DistanceFromUnplacedOrb(int targetIndex)
    {
        return 1000 + targetIndex;
    }

    private void LayoutEmptyOrbs(int seedCount, int previousCapacity, int capacity)
    {
        List<int> targetIndices = [];
        for (int index = seedCount; index < capacity; index++)
        {
            targetIndices.Add(index);
        }

        List<NBotanistCultivationOrb> unassigned = new(_emptyOrbs.Keys);
        List<int> remainingTargets = [];
        foreach (int targetIndex in targetIndices)
        {
            int matchIndex = unassigned.FindIndex(
                orb => _emptyOrbs.GetValueOrDefault(orb, -1) == targetIndex);
            if (matchIndex < 0)
            {
                remainingTargets.Add(targetIndex);
                continue;
            }

            NBotanistCultivationOrb orb = unassigned[matchIndex];
            unassigned.RemoveAt(matchIndex);
            int previousIndex = _emptyOrbs[orb];
            _emptyOrbs[orb] = targetIndex;
            orb.SetPlanted(null);
            AnimateOrbAlongArc(orb, previousIndex, previousCapacity, targetIndex, capacity);
        }

        unassigned.Sort((left, right) =>
            _emptyOrbs.GetValueOrDefault(left, -1).CompareTo(_emptyOrbs.GetValueOrDefault(right, -1)));
        for (int index = 0; index < unassigned.Count && index < remainingTargets.Count; index++)
        {
            NBotanistCultivationOrb orb = unassigned[index];
            int targetIndex = remainingTargets[index];
            int previousIndex = _emptyOrbs[orb];
            _emptyOrbs[orb] = targetIndex;
            orb.SetPlanted(null);
            AnimateOrbAlongArc(orb, previousIndex, previousCapacity, targetIndex, capacity);
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

    private void AnimateOrbAlongArc(
        NBotanistCultivationOrb orb,
        int previousIndex,
        int previousCapacity,
        int nextIndex,
        int nextCapacity)
    {
        if (previousIndex < 0)
        {
            TweenToSlot(orb, SlotPosition(nextIndex, nextCapacity));
            return;
        }

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
