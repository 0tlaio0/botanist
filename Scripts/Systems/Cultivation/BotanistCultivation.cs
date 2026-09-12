using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Rooms;

namespace Botanist.Scripts;

public sealed class PlantedSeed
{
    public required IBotanistSeedCard Seed { get; init; }
    public CardModel Card => Seed.Card;
    public Dictionary<BotanistElement, int> Remaining { get; } = new();

    public bool IsRipe => Remaining.Values.All(count => count <= 0);
}

public sealed class BotanistCultivation : CustomSingletonModel
{
    public const int InitialCapacity = 3;
    public const int MaxCapacity = 10;

    public static BotanistCultivation? Instance { get; private set; }

    private readonly Dictionary<ulong, List<PlantedSeed>> _plantedByPlayer = new();
    private readonly Dictionary<ulong, int> _capacityByPlayer = new();
    private readonly HashSet<CardModel> _pendingSeedPlays = [];

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
        if (Instance == null)
        {
            return Array.Empty<PlantedSeed>();
        }

        return Instance.GetOrCreate(player);
    }

    public static int GetCapacity(Player player)
    {
        return Instance?._capacityByPlayer.GetValueOrDefault(player.NetId, InitialCapacity)
            ?? InitialCapacity;
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

        Instance._capacityByPlayer[player.NetId] = next;
        Instance.Changed?.Invoke();
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

        List<PlantedSeed> planted = Instance.GetOrCreate(player);
        while (planted.Count > next)
        {
            PlantedSeed removed = planted[^1];
            planted.RemoveAt(planted.Count - 1);
            if (removed.Card.Pile != null)
            {
                await CardPileCmd.Add(removed.Card, PileType.Discard);
            }
        }

        Instance._capacityByPlayer[player.NetId] = next;
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

        List<PlantedSeed> planted = Instance.GetOrCreate(player);
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

        List<PlantedSeed> planted = Instance.GetOrCreate(player);
        return planted.Count == 0
            ? Task.CompletedTask
            : Instance.ReduceGrowth(choiceContext, player, planted.ToList(), amount);
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

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card is not BotanistCardModel botanistCard)
        {
            return;
        }

        // 结算顺序：先把这张牌的元素给培养区里已有的种子，再把新种子放进去。
        // 这样种子不会用自己的元素给自己扣点，但会喂到场上其他种子。
        await ApplyElement(choiceContext, botanistCard.Owner, botanistCard.Element);
    }

    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (cardPlay.Card.IsSeed())
        {
            _pendingSeedPlays.Add(cardPlay.Card);
        }

        return Task.CompletedTask;
    }

    public override async Task AfterCardChangedPiles(
        CardModel card,
        PileType oldPileType,
        AbstractModel? clonedBy)
    {
        if (oldPileType != PileType.Play || !_pendingSeedPlays.Remove(card))
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
        List<PlantedSeed> planted = GetOrCreate(player);
        if (planted.Any(item => item.Card == card))
        {
            return;
        }

        // 打出效果和元素结算完成后才判断空位，确保同一次结算中成熟的种子会先腾出位置。
        if (planted.Count >= GetCapacity(player))
        {
            if (card.Pile?.Type == PileType.Play)
            {
                await CardPileCmd.Add(card, PileType.Discard);
            }

            return;
        }

        // 卡牌模型留在标准 PlayPile 中作为战斗内的隐藏培育存储。
        // 这样 card.Pile 可正常解析，成熟时才能稳定地移入弃牌堆。
        NCard? node = NCard.FindOnTable(card);
        PlantedSeed plantedSeed = new() { Seed = seed };
        foreach (KeyValuePair<BotanistElement, int> requirement in seed.Requirements)
        {
            plantedSeed.Remaining[requirement.Key] = requirement.Value;
        }

        planted.Add(plantedSeed);
        Changed?.Invoke();

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

        List<PlantedSeed> planted = GetOrCreate(player);
        if (planted.Count == 0)
        {
            return;
        }

        foreach (PlantedSeed seed in planted)
        {
            if (element == BotanistElement.Aether)
            {
                foreach (BotanistElement required in seed.Remaining.Keys.ToList())
                {
                    seed.Remaining[required] = Math.Max(0, seed.Remaining[required] - 1);
                }
            }
            else if (seed.Remaining.ContainsKey(element))
            {
                seed.Remaining[element] = Math.Max(0, seed.Remaining[element] - 1);
            }
        }

        Changed?.Invoke();
        await RipenReady(choiceContext, player);
    }

    private async Task ReduceGrowth(
        PlayerChoiceContext choiceContext,
        Player player,
        IReadOnlyList<PlantedSeed> seeds,
        int amount)
    {
        bool changed = false;
        foreach (PlantedSeed seed in seeds)
        {
            foreach (KeyValuePair<BotanistElement, int> requirement in seed.Seed.Requirements)
            {
                int current = seed.Remaining.GetValueOrDefault(requirement.Key);
                int next = Math.Max(0, current - amount);
                if (next == current)
                {
                    continue;
                }

                seed.Remaining[requirement.Key] = next;
                changed = true;
            }
        }

        if (!changed)
        {
            return;
        }

        Changed?.Invoke();
        await RipenReady(choiceContext, player);
    }

    private async Task RipenReady(PlayerChoiceContext choiceContext, Player player)
    {
        List<PlantedSeed> planted = GetOrCreate(player);
        List<PlantedSeed> ripe = planted.Where(seed => seed.IsRipe).ToList();
        foreach (PlantedSeed seed in ripe)
        {
            planted.Remove(seed);
            await seed.Seed.OnRipen(choiceContext);
            if (seed.Card.Pile != null)
            {
                await CardPileCmd.Add(seed.Card, PileType.Discard);
            }

            if (player.Creature.GetPower<BotanistSpecimenCasePower>() is { } specimenCase)
            {
                for (int i = 0; i < specimenCase.Amount; i++)
                {
                    await AddSpecimenToDraw(player, seed.Seed);
                }
            }

            BotanistHeavenPalmBottle? bottle = player.GetRelic<BotanistHeavenPalmBottle>();
            if (bottle != null)
            {
                await bottle.OnSeedCultivated(choiceContext);
            }
        }

        if (ripe.Count > 0)
        {
            Changed?.Invoke();
        }
    }

    private static async Task AddSpecimenToDraw(Player player, IBotanistSeedCard seed)
    {
        ICombatState? combatState = player.Creature.CombatState;
        if (combatState == null)
        {
            return;
        }

        BotanistSpecimen specimen = combatState.CreateCard<BotanistSpecimen>(player);
        specimen.SetCopiedSeed(seed);
        await CardPileCmd.AddGeneratedCardToCombat(
            specimen,
            PileType.Draw,
            player,
            CardPilePosition.Random);
    }

    private void ResetCombatState()
    {
        _plantedByPlayer.Clear();
        _capacityByPlayer.Clear();
        _pendingSeedPlays.Clear();
        Changed?.Invoke();
    }

    private List<PlantedSeed> GetOrCreate(Player player)
    {
        if (!_plantedByPlayer.TryGetValue(player.NetId, out List<PlantedSeed>? planted))
        {
            planted = new List<PlantedSeed>();
            _plantedByPlayer[player.NetId] = planted;
        }

        return planted;
    }
}
