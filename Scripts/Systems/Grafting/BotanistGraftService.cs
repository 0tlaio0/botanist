using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace Botanist.Scripts;

/// <summary>集中处理嫁接资格、需求计算、快照重建和正式结算。</summary>
public static class BotanistGraftService
{
    /// <summary>判断卡牌是否可以作为砧木；植物组织也可以作为砧木。</summary>
    public static bool CanBeGraftRoot(CardModel root)
    {
        return root is not null && root.IsSeed();
    }

    /// <summary>判断卡牌是否可以作为接穗。</summary>
    public static bool CanBeScion(CardModel scion)
    {
        if (scion is null ||
            !scion.IsSeed() ||
            scion is BotanistPlantTissue ||
            scion.Enchantment is not null ||
            !scion.IsRemovable)
        {
            return false;
        }

        return GetGraftModifier(scion) is not { } modifier ||
               modifier.Snapshots.Count == 0;
    }

    /// <summary>判断两张牌实体当前是否构成一次合法嫁接。</summary>
    public static bool CanGraft(CardModel root, CardModel scion)
    {
        if (root is null ||
            scion is null ||
            ReferenceEquals(root, scion) ||
            !CanBeGraftRoot(root) ||
            !CanBeScion(scion))
        {
            return false;
        }

        Player? rootOwner = GetOwnerOrNull(root);
        Player? scionOwner = GetOwnerOrNull(scion);
        if (rootOwner is null ||
            scionOwner is null ||
            !ReferenceEquals(rootOwner, scionOwner))
        {
            return false;
        }

        if (root.Pile?.Type != PileType.Deck || scion.Pile?.Type != PileType.Deck)
        {
            return false;
        }

        BotanistGraftSnapshot snapshot = CreateSnapshot(scion);
        return TryCreateSnapshotCard(snapshot, out CardModel? card) &&
               card is not null &&
               card.AsSeed() is not null;
    }

    /// <summary>按嫁接顺序返回砧木保存的接穗快照。</summary>
    public static IReadOnlyList<BotanistGraftSnapshot> GetGraftSnapshots(CardModel root)
    {
        return root is null
            ? Array.Empty<BotanistGraftSnapshot>()
            : GetGraftModifier(root)?.Snapshots ?? Array.Empty<BotanistGraftSnapshot>();
    }

    /// <summary>返回卡牌的基础需求与全部嫁接贡献之和。</summary>
    public static IReadOnlyList<KeyValuePair<BotanistElement, int>> GetEffectiveRequirements(CardModel card)
    {
        if (card is null || card.AsSeed() is not { } seed)
        {
            return Array.Empty<KeyValuePair<BotanistElement, int>>();
        }

        Dictionary<BotanistElement, int> totals = [];
        List<BotanistElement> order = [];

        AddRequirements(seed.Requirements, totals, order);
        foreach (BotanistGraftSnapshot snapshot in GetGraftSnapshots(card))
        {
            if (!TryCreateSnapshotCard(snapshot, out CardModel? scionCard) ||
                scionCard is null ||
                scionCard.AsSeed() is not { } scionSeed)
            {
                continue;
            }

            AddRequirements(
                scionSeed.Requirements.Select(
                    requirement => new KeyValuePair<BotanistElement, int>(
                        requirement.Key,
                        CeilHalf(requirement.Value))),
                totals,
                order);
        }

        return order
            .Where(element => totals[element] > 0)
            .Select(element => new KeyValuePair<BotanistElement, int>(element, totals[element]))
            .ToList();
    }

    /// <summary>构造包含砧木成长与全部嫁接收穗成长的预览摘要。</summary>
    public static string BuildRipenSummary(CardModel card)
    {
        if (card is null || card.AsSeed() is not { } seed)
        {
            return string.Empty;
        }

        List<string> lines = [];
        if (!string.IsNullOrWhiteSpace(seed.RipenSummary))
        {
            lines.Add(seed.RipenSummary);
        }

        foreach (BotanistGraftSnapshot snapshot in GetGraftSnapshots(card))
        {
            if (TryCreateSnapshotCard(snapshot, out CardModel? scionCard) &&
                scionCard is not null &&
                scionCard.AsSeed() is { } scionSeed)
            {
                lines.Add($"嫁接·{scionCard.Title}：{scionSeed.RipenSummary}");
            }
            else
            {
                lines.Add(BuildFallbackSnapshotLine(snapshot));
            }
        }

        return string.Join("\n", lines);
    }

    /// <summary>复制砧木并临时追加一枚接穗快照，用于实时结果预览。</summary>
    public static CardModel? CreatePreviewCard(CardModel root, CardModel scion)
    {
        if (!CanGraft(root, scion))
        {
            return null;
        }

        CardModel preview = (CardModel)root.MutableClone();
        GetOrCreateModifier(preview).Append(CreateSnapshot(scion));
        return preview;
    }

    /// <summary>按快照重建一枚临时接穗卡；未知或已不再属于种子的卡牌返回空。</summary>
    public static CardModel? CreateScionCard(CardModel root, BotanistGraftSnapshot snapshot)
    {
        if (!TryCreateSnapshotCard(snapshot, out CardModel? card) ||
            card is null ||
            card.AsSeed() is null)
        {
            return null;
        }

        Player? owner = GetOwnerOrNull(root);
        if (owner is null || owner.Creature.CombatState is null)
        {
            return null;
        }

        card.Owner = owner;
        return card;
    }

    /// <summary>
    /// 正式应用嫁接：先给真实砧木追加记录，再从牌组移除接穗。
    /// 移除抛错时撤回刚追加的记录；成功后不做任何额外全局登记。
    /// </summary>
    public static async Task<bool> ApplyGraftAsync(CardModel root, CardModel scion)
    {
        if (!CanGraft(root, scion))
        {
            return false;
        }

        BotanistGraftSnapshot snapshot = CreateSnapshot(scion);
        if (!TryCreateSnapshotCard(snapshot, out CardModel? scionCard) ||
            scionCard is null ||
            scionCard.AsSeed() is null)
        {
            return false;
        }

        BotanistGraftModifier modifier = GetOrCreateModifier(root);
        modifier.Append(snapshot);
        try
        {
            await CardPileCmd.RemoveFromDeck(scion, showPreview: false);
        }
        catch
        {
            if (modifier.RemoveLast(snapshot) && modifier.Snapshots.Count == 0)
            {
                CardModifier.RemoveModifier(root, modifier);
            }

            throw;
        }

        return true;
    }

    internal static string BuildGraftGrowthLine(BotanistGraftSnapshot snapshot, CardModel? owner)
    {
        if (!TryCreateSnapshotCard(snapshot, out CardModel? scionCard) ||
            scionCard is null ||
            scionCard.AsSeed() is not { } scionSeed)
        {
            return BuildFallbackSnapshotLine(snapshot);
        }

        Player? player = owner is null ? null : GetOwnerOrNull(owner);
        if (player is not null && scionCard.Owner is null)
        {
            scionCard.Owner = player;
        }

        LocString loc = new("card_modifiers", $"{GetModifierLocKey()}.growthLine");
        loc.Add("Scion", scionCard.Title);
        loc.Add("Growth", scionSeed.RipenSummary);
        return loc.GetFormattedText();
    }

    private static BotanistGraftSnapshot CreateSnapshot(CardModel scion)
    {
        return new BotanistGraftSnapshot(scion.Id, scion.CurrentUpgradeLevel);
    }

    private static bool TryCreateSnapshotCard(
        BotanistGraftSnapshot snapshot,
        out CardModel? card)
    {
        card = null;
        if (snapshot is null || snapshot.UpgradeLevel < 0)
        {
            return false;
        }

        CardModel? canonical = ModelDb.GetByIdOrNull<CardModel>(snapshot.ModelId);
        if (canonical is null)
        {
            return false;
        }

        card = canonical.ToMutable();
        int upgradeLevel = Math.Min(snapshot.UpgradeLevel, card.MaxUpgradeLevel);
        for (int index = 0; index < upgradeLevel; index++)
        {
            card.UpgradeInternal();
            card.FinalizeUpgradeInternal();
        }

        return true;
    }

    private static BotanistGraftModifier? GetGraftModifier(CardModel card)
    {
        return CardModifier.DirectModifiers(card)
            .OfType<BotanistGraftModifier>()
            .FirstOrDefault();
    }

    private static BotanistGraftModifier GetOrCreateModifier(CardModel card)
    {
        BotanistGraftModifier? existing = GetGraftModifier(card);
        if (existing is not null)
        {
            return existing;
        }

        BotanistGraftModifier created =
            ModelDbExtensions.CardModifier<BotanistGraftModifier>(mutableClone: true);
        CardModifier.AddModifier(card, created);
        return created;
    }

    private static Player? GetOwnerOrNull(CardModel card)
    {
        return card.IsMutable ? card.Owner : null;
    }

    private static void AddRequirements(
        IEnumerable<KeyValuePair<BotanistElement, int>> requirements,
        Dictionary<BotanistElement, int> totals,
        List<BotanistElement> order)
    {
        foreach (KeyValuePair<BotanistElement, int> requirement in requirements)
        {
            if (requirement.Key == BotanistElement.None || requirement.Value <= 0)
            {
                continue;
            }

            if (!totals.ContainsKey(requirement.Key))
            {
                order.Add(requirement.Key);
            }

            totals[requirement.Key] = totals.GetValueOrDefault(requirement.Key) + requirement.Value;
        }
    }

    private static int CeilHalf(int value)
    {
        return value <= 0 ? 0 : (value + 1) / 2;
    }

    private static string BuildFallbackSnapshotLine(BotanistGraftSnapshot snapshot)
    {
        string subKey = snapshot.ModelId.Category == "UNKNOWN"
            ? "unknownSnapshot"
            : "missingSnapshot";
        LocString loc = new("card_modifiers", $"{GetModifierLocKey()}.{subKey}");
        loc.Add("ModelId", snapshot.ModelId.ToString());
        return loc.GetFormattedText();
    }

    private static string GetModifierLocKey()
    {
        return ModelDb.GetId<BotanistGraftModifier>().Entry;
    }
}
