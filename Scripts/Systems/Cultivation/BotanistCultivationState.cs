using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace Botanist.Scripts;

/// <summary>保存单场战斗中的培育区、费用状态和卡牌流转状态。</summary>
internal sealed class BotanistCultivationState
{
    private readonly Dictionary<ulong, List<PlantedSeed>> _plantedByPlayer = new();
    private readonly Dictionary<ulong, int> _capacityByPlayer = new();
    private readonly Dictionary<ulong, int> _seedsCultivatedThisTurn = new();
    private readonly HashSet<CardModel> _pendingSeedPlays = [];
    private readonly HashSet<CardModel> _growthFreeCards = [];

    public List<PlantedSeed> GetOrCreatePlanted(Player player)
    {
        if (!_plantedByPlayer.TryGetValue(player.NetId, out List<PlantedSeed>? planted))
        {
            planted = [];
            _plantedByPlayer[player.NetId] = planted;
        }

        return planted;
    }

    public int GetCapacity(Player player)
    {
        return _capacityByPlayer.GetValueOrDefault(player.NetId, BotanistCultivation.InitialCapacity);
    }

    public void SetCapacity(Player player, int capacity)
    {
        _capacityByPlayer[player.NetId] = capacity;
    }

    public int GetSeedsCultivatedThisTurn(Player player)
    {
        return _seedsCultivatedThisTurn.GetValueOrDefault(player.NetId);
    }

    public void IncrementSeedsCultivatedThisTurn(Player player)
    {
        _seedsCultivatedThisTurn[player.NetId] = GetSeedsCultivatedThisTurn(player) + 1;
    }

    public void ResetSeedsCultivatedThisTurn(Player player)
    {
        _seedsCultivatedThisTurn.Remove(player.NetId);
    }

    public void MarkPendingSeed(CardModel card)
    {
        _pendingSeedPlays.Add(card);
    }

    public bool ConsumePendingSeed(CardModel card)
    {
        return _pendingSeedPlays.Remove(card);
    }

    public void MarkGrowthFree(CardModel card)
    {
        _growthFreeCards.Add(card);
    }

    public bool IsGrowthFree(CardModel card)
    {
        return _growthFreeCards.Contains(card);
    }

    public bool ConsumeGrowthFree(CardModel card)
    {
        return _growthFreeCards.Remove(card);
    }

    public void ClearTurnState()
    {
        _seedsCultivatedThisTurn.Clear();
        _growthFreeCards.Clear();
    }

    public void Reset()
    {
        _plantedByPlayer.Clear();
        _capacityByPlayer.Clear();
        _seedsCultivatedThisTurn.Clear();
        _pendingSeedPlays.Clear();
        _growthFreeCards.Clear();
    }
}
