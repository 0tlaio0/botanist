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

        return card.Owner.Creature.GetPower<BotanistSlowReleaseFertilizerPower>()?.Amount ?? 0;
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
            PlantedSeed removed = planted[^1];
            planted.RemoveAt(planted.Count - 1);
            await removed.Seed.ResolveAfterCultivation();
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

        if (botanistCard.AsSeed() is not null &&
            botanistCard.Owner.Creature.GetPower<BotanistSlowReleaseFertilizerPower>() is { } fertilizer)
        {
            _state.MarkSeedRequirementReduction(botanistCard, fertilizer.Amount);
            await PowerCmd.Remove(fertilizer);
            BotanistCardChrome.RefreshSeedPreviews(botanistCard.Owner);
        }

        if (!IsGrowthFree(botanistCard) ||
            botanistCard.AsSeed() is not { } seed ||
            !HasSpace(botanistCard.Owner))
        {
            return;
        }

        // 成长需求已视为 0：像正常种子一样检查空位，成功入场后立即成熟。
        _state.ConsumeGrowthFree(botanistCard);
        _state.ConsumeSeedRequirementReduction(botanistCard);
        PlantedSeed ripeSeed = new() { Seed = seed };
        foreach (KeyValuePair<BotanistElement, int> requirement in seed.Requirements)
        {
            ripeSeed.Remaining[requirement.Key] = 0;
        }

        List<PlantedSeed> planted = _state.GetOrCreatePlanted(botanistCard.Owner);
        planted.Add(ripeSeed);
        Changed?.Invoke();
        await TriggerSeedEntryPowers(choiceContext, botanistCard.Owner, planted);
        await RipenReady(choiceContext, botanistCard.Owner);
    }

    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (cardPlay.Card.IsSeed())
        {
            _state.MarkPendingSeed(cardPlay.Card);
        }

        return Task.CompletedTask;
    }

    public override async Task AfterCardChangedPiles(
        CardModel card,
        PileType oldPileType,
        AbstractModel? clonedBy)
    {
        if (oldPileType != PileType.Play || !_state.ConsumePendingSeed(card))
        {
            return;
        }

        if (card.Pile?.Type == PileType.Play && card.AsSeed() is { } seed)
        {
            await TryPlant(seed);
        }
    }

    private async Task TryPlant(IBotanistSeedCard seed)
    {
        CardModel card = seed.Card;
        Player player = card.Owner;
        int requirementReduction = _state.ConsumeSeedRequirementReduction(card);
        List<PlantedSeed> planted = _state.GetOrCreatePlanted(player);
        if (planted.Any(item => item.Card == card))
        {
            return;
        }

        // 打出效果和元素结算完成后才判断空位，确保同一次结算中成熟的种子会先腾出位置。
        if (planted.Count >= GetCapacity(player))
        {
            await seed.ResolveAfterCultivation();
            return;
        }

        // 卡牌模型留在标准 PlayPile 中作为战斗内的隐藏培育存储。
        // 这样 card.Pile 可正常解析，成熟时才能稳定地移入弃牌堆。
        NCard? node = NCard.FindOnTable(card);
        PlantedSeed plantedSeed = new() { Seed = seed };
        foreach (KeyValuePair<BotanistElement, int> requirement in seed.Requirements)
        {
            plantedSeed.Remaining[requirement.Key] =
                Math.Max(0, requirement.Value - requirementReduction);
        }

        planted.Add(plantedSeed);
        Changed?.Invoke();
        await TriggerSeedEntryPowers(new ThrowingPlayerChoiceContext(), player, planted);
        await RipenReady(choiceContext: new ThrowingPlayerChoiceContext(), player);

        if (node != null && GodotObject.IsInstanceValid(node))
        {
            node.QueueFreeSafely();
        }
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

            foreach (KeyValuePair<BotanistElement, int> requirement in target.Seed.Requirements)
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
                foreach (BotanistElement element in planted[newSeedIndex].Seed.Requirements.Select(
                             requirement => requirement.Key))
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
            planted.Remove(seed);
            await BotanistGrowthResolution.ResolveAsync(seed.Seed, choiceContext);
            await seed.Seed.ResolveAfterCultivation();

            MarkSeedCultivated(player);
            await BotanistSeedCultivationEffects.TriggerAsync(choiceContext, player, seed);
        }

        if (ripe.Count > 0)
        {
            Changed?.Invoke();
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
