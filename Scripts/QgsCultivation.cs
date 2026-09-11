using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Rooms;

namespace Qgs.Scripts;

public sealed class PlantedSeed
{
    public required QgsSeedCardModel Card { get; init; }
    public Dictionary<QgsElement, int> Remaining { get; } = new();

    public bool IsRipe => Remaining.Values.All(count => count <= 0);
}

public sealed class QgsCultivation : CustomSingletonModel
{
    public const int Capacity = 3;

    public static QgsCultivation? Instance { get; private set; }

    private readonly Dictionary<ulong, List<PlantedSeed>> _plantedByPlayer = new();
    private readonly Dictionary<ulong, CardPile> _piles = new();

    public event Action? Changed;

    public QgsCultivation() : base(HookType.Combat)
    {
        Instance = this;
    }

    public static bool HasSpace(Player? player)
    {
        if (player == null)
        {
            return false;
        }

        return GetPlanted(player).Count < Capacity;
    }

    public static IReadOnlyList<PlantedSeed> GetPlanted(Player player)
    {
        if (Instance == null)
        {
            return Array.Empty<PlantedSeed>();
        }

        return Instance.GetOrCreate(player);
    }

    public override Task BeforeCombatStart()
    {
        ResetCombatState();
        NQgsCultivationZone.TryAttach();
        return Task.CompletedTask;
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        ResetCombatState();
        NQgsCultivationZone.Detach();
        return Task.CompletedTask;
    }

    public override Task AfterRoomEntered(AbstractRoom room)
    {
        if (room is CombatRoom)
        {
            NQgsCultivationZone.TryAttach();
        }

        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card is not QgsCardModel qgsCard)
        {
            return;
        }

        // 结算顺序：先把这张牌的元素给培养区里已有的种子，再把新种子放进去。
        // 这样种子不会用自己的元素给自己扣点，但会喂到场上其他种子。
        await ApplyElement(choiceContext, qgsCard.Owner, qgsCard.Element);

        if (qgsCard is QgsSeedCardModel seed)
        {
            await TryPlant(seed);
        }
    }

    private async Task TryPlant(QgsSeedCardModel seed)
    {
        Player player = seed.Owner;
        List<PlantedSeed> planted = GetOrCreate(player);
        if (planted.Count >= Capacity || planted.Any(item => item.Card == seed))
        {
            return;
        }

        NCard? node = NCard.FindOnTable(seed);
        CardPile pile = GetOrCreatePile(player);
        await CardPileCmd.Add(seed, pile, CardPilePosition.Bottom, null, skipVisuals: true);
        if (node != null && GodotObject.IsInstanceValid(node))
        {
            node.QueueFreeSafely();
        }

        PlantedSeed plantedSeed = new() { Card = seed };
        foreach (KeyValuePair<QgsElement, int> requirement in seed.Requirements)
        {
            plantedSeed.Remaining[requirement.Key] = requirement.Value;
        }

        planted.Add(plantedSeed);
        Changed?.Invoke();
    }

    private async Task ApplyElement(PlayerChoiceContext choiceContext, Player player, QgsElement element)
    {
        if (element == QgsElement.None)
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
            if (element == QgsElement.Aether)
            {
                foreach (QgsElement required in seed.Remaining.Keys.ToList())
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

    private async Task RipenReady(PlayerChoiceContext choiceContext, Player player)
    {
        List<PlantedSeed> planted = GetOrCreate(player);
        List<PlantedSeed> ripe = planted.Where(seed => seed.IsRipe).ToList();
        foreach (PlantedSeed seed in ripe)
        {
            planted.Remove(seed);
            await seed.Card.OnRipen(choiceContext);
            if (seed.Card.Pile != null)
            {
                await CardPileCmd.Add(seed.Card, PileType.Discard);
            }
        }

        if (ripe.Count > 0)
        {
            Changed?.Invoke();
        }
    }

    private void ResetCombatState()
    {
        _plantedByPlayer.Clear();
        _piles.Clear();
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

    private CardPile GetOrCreatePile(Player player)
    {
        if (!_piles.TryGetValue(player.NetId, out CardPile? pile))
        {
            pile = new CardPile(PileType.Play);
            _piles[player.NetId] = pile;
        }

        return pile;
    }
}
