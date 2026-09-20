using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Rooms;

namespace Botanist.Scripts;

public sealed class BotanistCultivation : CustomSingletonModel
{
    public const int InitialCapacity = 3;
    public const int MaxCapacity = 10;

    public static BotanistCultivation? Instance { get; private set; }

    private readonly BotanistCultivationState _state = new();

    public event Action? Changed;

    public BotanistCultivation() : base(HookType.Combat)
    {
        Instance = this;
    }

    public static bool HasSpace(Player? player)
    {
        if (player == null)
        {
            return false;
        }

        return GetPlanted(player).Count < GetCapacity(player);
    }

    public static IReadOnlyList<PlantedSeed> GetPlanted(Player player)
    {
        return Instance == null
            ? Array.Empty<PlantedSeed>()
            : Instance._state.GetOrCreatePlanted(player);
    }

    public static int GetCapacity(Player player)
    {
        return Instance?._state.GetCapacity(player) ?? InitialCapacity;
    }

    public static void AddCapacity(Player player, int amount)
    {
        if (Instance == null || amount <= 0)
        {
            return;
        }

        int current = GetCapacity(player);
        int next = Math.Min(MaxCapacity, current + amount);
        if (next == current)
        {
            return;
        }

        Instance._state.SetCapacity(player, next);
        Instance.Changed?.Invoke();
    }

    public static void MarkGrowthFree(CardModel card)
    {
        if (Instance == null || !card.IsSeed())
        {
            return;
        }

        Instance._state.MarkGrowthFree(card);
    }

    public static bool IsGrowthFree(CardModel card)
    {
        return Instance?._state.IsGrowthFree(card) ?? false;
    }

    public static int GetPendingSeedRequirementReduction(CardModel card)
    {
        if (Instance == null || card.Pile == null)
        {
            return 0;
        }

        int stored = Instance._state.GetSeedRequirementReduction(card);
        return stored > 0
            ? stored
            : card.Owner.Creature.GetPower<BotanistSlowReleaseFertilizerPower>()?.Amount ?? 0;
    }

    public static int GetSeedsCultivatedThisTurn(Player player)
    {
        return Instance?._state.GetSeedsCultivatedThisTurn(player) ?? 0;
    }

    public static int GetSeedsCultivatedThisCombat(Player player)
    {
        return Instance?._state.GetSeedsCultivatedThisCombat(player) ?? 0;
    }

    public static int GetFireAbsorbedThisTurn(Player player)
    {
        return Instance?._state.GetFireAbsorbedThisTurn(player) ?? 0;
    }

    public static async Task RemoveCapacity(Player player, int amount)
    {
        if (Instance == null || amount <= 0)
        {
            return;
        }

        int current = GetCapacity(player);
        int next = Math.Max(0, current - amount);
        if (next == current)
        {
            return;
        }

        List<PlantedSeed> planted = Instance._state.GetOrCreatePlanted(player);
        while (planted.Count > next)
        {
            CardModel removedCard = planted[^1].Card;
            planted.RemoveAt(planted.Count - 1);
            await Instance.MoveOriginalIfNoCultivationInstances(removedCard);
        }

        Instance._state.SetCapacity(player, next);
        Instance.Changed?.Invoke();
    }

    public static Task ReduceFirstSeedGrowth(
        PlayerChoiceContext choiceContext,
        Player player,
        int amount = 1)
    {
        if (Instance == null || amount <= 0)
        {
            return Task.CompletedTask;
        }

        List<PlantedSeed> planted = Instance._state.GetOrCreatePlanted(player);
        if (planted.Count == 0)
        {
            return Task.CompletedTask;
        }

        return Instance.ReduceGrowth(choiceContext, player, [planted[0]], amount);
    }

    public static Task ReduceLastSeedGrowth(
        PlayerChoiceContext choiceContext,
        Player player,
        int amount = 1)
    {
        if (Instance == null || amount <= 0)
        {
            return Task.CompletedTask;
        }

        List<PlantedSeed> planted = Instance._state.GetOrCreatePlanted(player);
        return planted.Count == 0
            ? Task.CompletedTask
            : Instance.ReduceGrowth(choiceContext, player, [planted[^1]], amount);
    }

    public static Task ReduceAllSeedsGrowth(
        PlayerChoiceContext choiceContext,
        Player player,
        int amount = 1)
    {
        if (Instance == null || amount <= 0)
        {
            return Task.CompletedTask;
        }

        List<PlantedSeed> planted = Instance._state.GetOrCreatePlanted(player);
        return planted.Count == 0
            ? Task.CompletedTask
            : Instance.ReduceGrowth(choiceContext, player, planted.ToList(), amount);
    }

    public static Task ReduceHighestSeedRequirements(
        PlayerChoiceContext choiceContext,
        Player player,
        int amount)
    {
        if (Instance == null || amount <= 0)
        {
            return Task.CompletedTask;
        }

        List<PlantedSeed> planted = Instance._state.GetOrCreatePlanted(player);
        Dictionary<PlantedSeed, Dictionary<BotanistElement, int>> reductions = [];
        foreach (PlantedSeed seed in planted)
        {
            BotanistElement? highest = null;
            int highestValue = 0;
            foreach (BotanistElement element in BotanistElements.Ordered)
            {
                int value = seed.Remaining.GetValueOrDefault(element);
                if (value > highestValue)
                {
                    highestValue = value;
                    highest = element;
                }
            }

            if (highest is { } highestElement && highestValue > 0)
            {
                AddScheduledReduction(reductions, seed, highestElement, amount);
            }
        }

        return Instance.ApplyScheduledGrowthReductions(choiceContext, player, reductions);
    }

    public static Task ReduceRandomRequirementOfSeed(
        PlayerChoiceContext choiceContext,
        Player player,
        PlantedSeed target,
        int amount)
    {
        if (Instance == null || amount <= 0)
        {
            return Task.CompletedTask;
        }

        List<PlantedSeed> planted = Instance._state.GetOrCreatePlanted(player);
        if (!planted.Contains(target))
        {
            return Task.CompletedTask;
        }

        List<BotanistElement> candidates = target.Remaining
            .Where(entry => entry.Value > 0)
            .Select(entry => entry.Key)
            .ToList();
        if (candidates.Count == 0)
        {
            return Task.CompletedTask;
        }

        BotanistElement selected = player.RunState.Rng.CombatCardSelection.NextItem(candidates);
        Dictionary<PlantedSeed, Dictionary<BotanistElement, int>> reductions = [];
        AddScheduledReduction(reductions, target, selected, amount);
        return Instance.ApplyScheduledGrowthReductions(choiceContext, player, reductions);
    }

    public static void ReduceSeedRequirement(CardModel card, int amount = 1)
    {
        if (Instance == null || amount <= 0 || !card.IsSeed())
        {
            return;
        }

        Instance._state.AddSeedRequirementReduction(card, amount);
        BotanistCardChrome.RefreshSeedPreviews(card.Owner);
    }

    public static async Task<bool> ConsumeLastSeed(PlayerChoiceContext choiceContext, Player player)
    {
        if (Instance == null)
        {
            return false;
        }

        List<PlantedSeed> planted = Instance._state.GetOrCreatePlanted(player);
        if (planted.Count == 0)
        {
            return false;
        }

        PlantedSeed removed = planted[^1];
        planted.RemoveAt(planted.Count - 1);

        if (!Instance.HasCultivationInstances(removed.Card) &&
            removed.Card.Pile?.Type == PileType.Play)
        {
            await CardCmd.Exhaust(choiceContext, removed.Card);
        }

        Instance.Changed?.Invoke();
        return true;
    }

    public static async Task<int> ConsumeAllSeeds(PlayerChoiceContext choiceContext, Player player)
    {
        int consumed = 0;
        while (await ConsumeLastSeed(choiceContext, player))
        {
            consumed++;
        }

        return consumed;
    }

    public static Task AbsorbElement(
        PlayerChoiceContext choiceContext,
        Player player,
        BotanistElement element)
    {
        return Instance == null
            ? Task.CompletedTask
            : Instance.ApplyElement(choiceContext, player, element);
    }

    public override Task BeforeCombatStart()
    {
        ResetCombatState();
        NBotanistCultivationZone.TryAttach();
        return Task.CompletedTask;
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        ResetCombatState();
        NBotanistCultivationZone.Detach();
        return Task.CompletedTask;
    }

    public override Task AfterRoomEntered(AbstractRoom room)
    {
        if (room is CombatRoom)
        {
            NBotanistCultivationZone.TryAttach();
        }

        return Task.CompletedTask;
    }

    public override Task BeforeSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Player)
        {
            foreach (Creature participant in participants)
            {
                if (!participant.IsPlayer || participant.Player is not { } player)
                {
                    continue;
                }

                _state.ResetSeedsCultivatedThisTurn(player);
                BotanistSeedBank.RefreshInHand(player);
            }

            _state.ClearTurnState();
        }

        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card is not BotanistCardModel botanistCard)
        {
            return;
        }

        // 结算顺序：先把这张牌的元素给培养区里已有的种子，再把新种子放进去。
        // 这样种子不会用自己的元素给自己扣点，但会喂到场上其他种子。
        await ApplyElement(choiceContext, botanistCard.Owner, botanistCard.Element);

        if (botanistCard.AsSeed() is null)
        {
            return;
        }

        if (_state.GetSeedRequirementReduction(botanistCard) <= 0 &&
            botanistCard.Owner.Creature.GetPower<BotanistSlowReleaseFertilizerPower>() is { } fertilizer)
        {
            _state.MarkSeedRequirementReduction(botanistCard, fertilizer.Amount);
        }

        await AddCultivationInstance(choiceContext, botanistCard);

        if (!cardPlay.IsLastInSeries)
        {
            return;
        }

        _state.ConsumeGrowthFree(botanistCard);
        _state.ConsumeSeedRequirementReduction(botanistCard);
        if (botanistCard.Owner.Creature.GetPower<BotanistSlowReleaseFertilizerPower>() is { } activeFertilizer)
        {
            await PowerCmd.Remove(activeFertilizer);
        }

        if (!HasCultivationInstances(botanistCard))
        {
            await MoveOriginalIfNoCultivationInstances(botanistCard);
        }

        BotanistCardChrome.RefreshSeedPreviews(botanistCard.Owner);
    }

    public override Task AfterCardChangedPiles(
        CardModel card,
        PileType oldPileType,
        AbstractModel? clonedBy)
    {
        if (card.AsSeed() != null &&
            card.Pile?.Type == PileType.Play &&
            HasCultivationInstances(card))
        {
            // 打牌流程最终会把 Play 结果牌重新写回 PlayPile；此时隐藏原卡节点。
            NCard? node = NCard.FindOnTable(card);
            node?.QueueFreeSafely();
        }

        return Task.CompletedTask;
    }

    private async Task AddCultivationInstance(
        PlayerChoiceContext choiceContext,
        BotanistCardModel sourceCard)
    {
        if (sourceCard.AsSeed() is not { } seed)
        {
            return;
        }

        Player player = sourceCard.Owner;
        List<PlantedSeed> planted = _state.GetOrCreatePlanted(player);
        if (planted.Count >= GetCapacity(player))
        {
            return;
        }

        // 重放只新增独立培育记录，不复制原卡实体。后加入的实例可以喂养此前实例。
        bool growthFree = IsGrowthFree(sourceCard);
        int requirementReduction = growthFree
            ? 0
            : _state.GetSeedRequirementReduction(sourceCard);
        if (player.GetRelic<BotanistBoneMeal>()?.TryApplyToFirstPlantedSeed() == true)
        {
            requirementReduction++;
        }

        PlantedSeed plantedSeed = new() { Seed = seed };
        foreach (KeyValuePair<BotanistElement, int> requirement in
                 BotanistGraftService.GetEffectiveRequirements(sourceCard))
        {
            plantedSeed.Remaining[requirement.Key] =
                growthFree ? 0 : Math.Max(0, requirement.Value - requirementReduction);
        }

        planted.Add(plantedSeed);
        Changed?.Invoke();
        await TriggerSeedEntryPowers(choiceContext, player, planted);
        await RipenReady(choiceContext, player);
    }

    private async Task ApplyElement(PlayerChoiceContext choiceContext, Player player, BotanistElement element)
    {
        if (element == BotanistElement.None)
        {
            return;
        }

        if (element == BotanistElement.Fire)
        {
            _state.IncrementFireAbsorbedThisTurn(player);
            BotanistSunlightObservation.RefreshInHand(player);
        }

        List<PlantedSeed> planted = _state.GetOrCreatePlanted(player);
        if (planted.Count == 0)
        {
            return;
        }

        Dictionary<PlantedSeed, Dictionary<BotanistElement, int>> reductions = [];
        for (int index = 0; index < planted.Count; index++)
        {
            if (element == BotanistElement.Aether)
            {
                foreach (BotanistElement required in planted[index].Remaining.Keys.ToList())
                {
                    ScheduleGrowthReduction(planted, index, required, 1, reductions);
                }
            }
            else
            {
                ScheduleGrowthReduction(planted, index, element, 1, reductions);
            }
        }

        await ApplyScheduledGrowthReductions(choiceContext, player, reductions);
    }

    private async Task ReduceGrowth(
        PlayerChoiceContext choiceContext,
        Player player,
        IReadOnlyList<PlantedSeed> targets,
        int amount)
    {
        List<PlantedSeed> planted = _state.GetOrCreatePlanted(player);
        Dictionary<PlantedSeed, Dictionary<BotanistElement, int>> reductions = [];
        foreach (PlantedSeed target in targets)
        {
            int index = planted.IndexOf(target);
            if (index < 0)
            {
                continue;
            }

            foreach (KeyValuePair<BotanistElement, int> requirement in
                     BotanistGraftService.GetEffectiveRequirements(target.Card))
            {
                ScheduleGrowthReduction(planted, index, requirement.Key, amount, reductions);
            }
        }

        await ApplyScheduledGrowthReductions(choiceContext, player, reductions);
    }

    private static void ScheduleGrowthReduction(
        IReadOnlyList<PlantedSeed> planted,
        int seedIndex,
        BotanistElement element,
        int amount,
        Dictionary<PlantedSeed, Dictionary<BotanistElement, int>> reductions)
    {
        if (amount <= 0)
        {
            return;
        }

        PlantedSeed source = planted[seedIndex];
        int sourceAvailable = source.Remaining.GetValueOrDefault(element);
        if (sourceAvailable <= 0)
        {
            return;
        }

        int requested = Math.Min(amount, sourceAvailable);
        int pending = requested;

        // 夺取只作用于紧邻的后一颗种子，且不会抵消后一颗种子自己的元素结算。
        if (seedIndex + 1 < planted.Count && planted[seedIndex + 1].Seed.StealsPreviousSeedGrowth)
        {
            PlantedSeed thief = planted[seedIndex + 1];
            int thiefAvailable = thief.Remaining.GetValueOrDefault(element);
            int thiefPending = thiefAvailable - GetScheduledReduction(reductions, thief, element);
            int stolen = Math.Min(pending, Math.Max(0, thiefPending));
            if (stolen > 0)
            {
                AddScheduledReduction(reductions, thief, element, stolen);
                pending -= stolen;
            }
        }

        if (pending > 0)
        {
            AddScheduledReduction(reductions, source, element, pending);
        }
    }

    private async Task ApplyScheduledGrowthReductions(
        PlayerChoiceContext choiceContext,
        Player player,
        Dictionary<PlantedSeed, Dictionary<BotanistElement, int>> reductions,
        bool ripenReady = true)
    {
        bool changed = false;
        foreach (KeyValuePair<PlantedSeed, Dictionary<BotanistElement, int>> entry in reductions)
        {
            foreach (KeyValuePair<BotanistElement, int> reduction in entry.Value)
            {
                int current = entry.Key.Remaining.GetValueOrDefault(reduction.Key);
                int next = Math.Max(0, current - reduction.Value);
                if (next == current)
                {
                    continue;
                }

                entry.Key.Remaining[reduction.Key] = next;
                changed = true;
            }
        }

        if (!changed)
        {
            return;
        }

        Changed?.Invoke();
        if (ripenReady)
        {
            await RipenReady(choiceContext, player);
        }
    }

    private async Task TriggerSeedEntryPowers(
        PlayerChoiceContext choiceContext,
        Player player,
        IReadOnlyList<PlantedSeed> planted)
    {
        int newSeedIndex = planted.Count - 1;
        if (newSeedIndex <= 0)
        {
            return;
        }

        Dictionary<PlantedSeed, Dictionary<BotanistElement, int>> reductions = [];
        foreach (BotanistWitherBloomFormPower power in
                 player.Creature.GetPowerInstances<BotanistWitherBloomFormPower>())
        {
            for (int trigger = 0; trigger < power.Amount; trigger++)
            {
                foreach (BotanistElement element in
                         BotanistGraftService.GetEffectiveRequirements(planted[newSeedIndex].Card)
                             .Select(requirement => requirement.Key))
                {
                    ScheduleGrowthReduction(
                        planted,
                        newSeedIndex - 1,
                        element,
                        1,
                        reductions);
                }
            }
        }

        if (reductions.Count == 0)
        {
            return;
        }

        await ApplyScheduledGrowthReductions(
            choiceContext,
            player,
            reductions,
            ripenReady: false);
    }

    private static int GetScheduledReduction(
        Dictionary<PlantedSeed, Dictionary<BotanistElement, int>> reductions,
        PlantedSeed seed,
        BotanistElement element)
    {
        return reductions.TryGetValue(seed, out Dictionary<BotanistElement, int>? elements)
            ? elements.GetValueOrDefault(element)
            : 0;
    }

    private static void AddScheduledReduction(
        Dictionary<PlantedSeed, Dictionary<BotanistElement, int>> reductions,
        PlantedSeed seed,
        BotanistElement element,
        int amount)
    {
        if (!reductions.TryGetValue(seed, out Dictionary<BotanistElement, int>? elements))
        {
            elements = [];
            reductions[seed] = elements;
        }

        elements[element] = elements.GetValueOrDefault(element) + amount;
    }

    private async Task RipenReady(PlayerChoiceContext choiceContext, Player player)
    {
        List<PlantedSeed> planted = _state.GetOrCreatePlanted(player);
        List<PlantedSeed> ripe = planted.Where(seed => seed.IsRipe).ToList();
        foreach (PlantedSeed seed in ripe)
        {
            if (!planted.Contains(seed))
            {
                continue;
            }

            int seedIndex = planted.IndexOf(seed);
            PlantedSeed? nextSeed = seedIndex + 1 < planted.Count
                ? planted[seedIndex + 1]
                : null;
            // 「成长」关键词的固有奖励：每颗种子成熟各获得1点能量。
            await PlayerCmd.GainEnergy(1m, player);
            await BotanistGrowthResolution.ResolveAsync(seed.Seed, choiceContext);
            planted.Remove(seed);
            // 首次成长即让原卡离场；后续培育记录继续独立结算。
            await MoveOriginalAfterGrowth(seed.Card);

            if (nextSeed is not null &&
                player.Creature.GetPower<BotanistFloweringSynchronizationPower>() is { } flowering)
            {
                await flowering.OnSeedCultivated(choiceContext, nextSeed);
            }

            MarkSeedCultivated(player);
            await BotanistSeedCultivationEffects.TriggerAsync(choiceContext, player, seed);
        }

        if (ripe.Count > 0)
        {
            Changed?.Invoke();
        }
    }

    private bool HasCultivationInstances(CardModel card)
    {
        return _state.GetOrCreatePlanted(card.Owner).Any(seed => seed.Card == card);
    }

    private async Task MoveOriginalIfNoCultivationInstances(CardModel card)
    {
        if (HasCultivationInstances(card) || card.AsSeed() is not { } seed)
        {
            return;
        }

        await seed.MoveToResultPileAfterCultivation();
    }

    private static async Task MoveOriginalAfterGrowth(CardModel card)
    {
        if (card.AsSeed() is { } seed)
        {
            await seed.MoveToResultPileAfterCultivation();
        }
    }

    private void MarkSeedCultivated(Player player)
    {
        _state.IncrementSeedsCultivatedThisCombat(player);
        _state.IncrementSeedsCultivatedThisTurn(player);
        BotanistSeedBank.RefreshInHand(player);
    }

    private void ResetCombatState()
    {
        _state.Reset();
        Changed?.Invoke();
    }
}
