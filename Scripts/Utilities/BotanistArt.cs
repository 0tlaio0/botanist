using System.Collections.Generic;
using Godot;

namespace Botanist.Scripts;

public static class BotanistArt
{
    public const string Character = "res://botanist/images/botanist_character.svg";
    public const string CharacterSelect = "res://botanist/images/character_select.svg";
    public const string CharacterSelectLocked = "res://botanist/images/character_select_locked.svg";
    public const string Energy = "res://botanist/images/energy.svg";
    public const string BigEnergy = "res://botanist/images/energy_big.svg";
    public const string Sprout = "res://botanist/images/elements/sprout.svg";
    public const string Sunward = "res://botanist/images/powers/sunward.svg";
    public const string EmptyOrb = "res://images/orbs/empty_orb.png";

    private static readonly Dictionary<string, Texture2D?> Cache = new();

    public static string ElementIcon(BotanistElement element) => element switch
    {
        BotanistElement.Earth => "res://botanist/images/elements/earth.svg",
        BotanistElement.Fire => "res://botanist/images/elements/fire.svg",
        BotanistElement.Water => "res://botanist/images/elements/water.svg",
        BotanistElement.Wind => "res://botanist/images/elements/wind.svg",
        BotanistElement.Aether => "res://botanist/images/elements/aether.svg",
        _ => string.Empty
    };

    public static Texture2D? Load(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        if (Cache.TryGetValue(path, out Texture2D? cached))
        {
            return cached;
        }

        Texture2D? texture = ResourceLoader.Exists(path)
            ? ResourceLoader.Load<Texture2D>(path)
            : null;
        Cache[path] = texture;
        return texture;
    }
}
