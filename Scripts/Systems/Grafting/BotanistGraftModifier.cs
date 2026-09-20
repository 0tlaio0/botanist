using System;
using System.Collections.Generic;
using System.Linq;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace Botanist.Scripts;

/// <summary>保存一次嫁接所需的接穗信息；不包含附魔，升级状态按嫁接时快照固定。</summary>
/// <param name="ModelId">接穗卡牌的模型 ID。</param>
/// <param name="UpgradeLevel">接穗在嫁接时的升级等级。</param>
public sealed record BotanistGraftSnapshot(ModelId ModelId, int UpgradeLevel);

/// <summary>
/// 作为砧木卡牌的永久修饰器保存有序接穗快照。
/// BaseLib 会在存档、联机和复制卡牌时通过该修饰器序列化或深拷贝记录。
/// </summary>
public sealed class BotanistGraftModifier : CardModifier, ICustomModel
{
    private const string SaveVersionKey = "Version";
    private const string ScionCountKey = "ScionCount";
    private const int CurrentSaveVersion = 1;

    private List<BotanistGraftSnapshot> _snapshots = [];

    /// <summary>按嫁接顺序返回只读接穗快照。</summary>
    public IReadOnlyList<BotanistGraftSnapshot> Snapshots => _snapshots.AsReadOnly();

    /// <summary>向记录末尾追加一枚接穗快照。</summary>
    public void Append(BotanistGraftSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        _snapshots.Add(snapshot);
    }

    /// <summary>仅当指定快照是末尾记录时撤回它，用于移除接穗失败时回滚。</summary>
    public bool RemoveLast(BotanistGraftSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        if (_snapshots.Count == 0 || _snapshots[^1] != snapshot)
        {
            return false;
        }

        _snapshots.RemoveAt(_snapshots.Count - 1);
        return true;
    }

    public override void StoreSaveData(ModifierSave save)
    {
        ArgumentNullException.ThrowIfNull(save);

        save.IntProperties.Clear();
        save.AdditionalProperties.Clear();
        save.IntProperties[SaveVersionKey] = CurrentSaveVersion;
        save.IntProperties[ScionCountKey] = _snapshots.Count;

        for (int index = 0; index < _snapshots.Count; index++)
        {
            BotanistGraftSnapshot snapshot = _snapshots[index];
            save.AdditionalProperties[GetModelIdKey(index)] = snapshot.ModelId.ToString();
            save.IntProperties[GetUpgradeLevelKey(index)] = snapshot.UpgradeLevel;
        }
    }

    public override void LoadSaveData(ModifierSave save)
    {
        ArgumentNullException.ThrowIfNull(save);

        _snapshots = [];
        int version = GetIntProperty(save, SaveVersionKey);
        if (version <= 0 || version > CurrentSaveVersion)
        {
            return;
        }

        int count = Math.Max(0, GetIntProperty(save, ScionCountKey));
        for (int index = 0; index < count; index++)
        {
            if (!save.AdditionalProperties.TryGetValue(GetModelIdKey(index), out string? rawModelId))
            {
                continue;
            }

            ModelId modelId = ParseModelId(rawModelId);
            int upgradeLevel = Math.Max(0, GetIntProperty(save, GetUpgradeLevelKey(index)));
            _snapshots.Add(new BotanistGraftSnapshot(modelId, upgradeLevel));
        }
    }

    public override void ModifyDescriptionPost(Creature? target, ref string description)
    {
        if (_snapshots.Count == 0)
        {
            return;
        }

        string graftLines = string.Join(
            "\n",
            _snapshots.Select(snapshot => BotanistGraftService.BuildGraftGrowthLine(snapshot, Owner)));
        description = string.IsNullOrEmpty(description)
            ? graftLines
            : $"{description}\n{graftLines}";
    }

    public override LocString GetLoc(string subKey = "description")
    {
        LocString loc = base.GetLoc(subKey);
        loc.Add("Count", _snapshots.Count);
        loc.Add(
            "Snapshots",
            string.Join(
                "\n",
                _snapshots.Select(snapshot => BotanistGraftService.BuildGraftGrowthLine(snapshot, Owner))));
        return loc;
    }

    protected override void DeepCloneFields()
    {
        base.DeepCloneFields();
        _snapshots = _snapshots
            .Select(snapshot => new BotanistGraftSnapshot(snapshot.ModelId, snapshot.UpgradeLevel))
            .ToList();
    }

    private static int GetIntProperty(ModifierSave save, string key)
    {
        return save.IntProperties.TryGetValue(key, out int value) ? value : 0;
    }

    private static string GetModelIdKey(int index)
    {
        return $"Scion.{index}.ModelId";
    }

    private static string GetUpgradeLevelKey(int index)
    {
        return $"Scion.{index}.UpgradeLevel";
    }

    private static ModelId ParseModelId(string rawModelId)
    {
        try
        {
            return ModelId.Deserialize(rawModelId);
        }
        catch (Exception)
        {
            // 无效记录仍保留在顺序中，卡面显示降级文案，避免读档时直接丢失后续快照位置。
            return new ModelId("UNKNOWN", rawModelId.Replace('.', '_'));
        }
    }
}
