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
    public const string SeedlingSeed = "res://botanist/images/elements/seedling_seed.png";
    public const string Sunward = "res://botanist/images/powers/sunward.svg";
    public const string Edelweiss = "res://botanist/images/cards/botanist/edelweiss.png";
    public const string IllusoryPhoenixGrass = "res://botanist/images/cards/botanist/illusory_phoenix_grass.png";
    public const string DevouringGoldVine = "res://botanist/images/cards/botanist/devouring_gold_vine.png";
    public const string NightGlowGrass = "res://botanist/images/cards/botanist/night_glow_grass.png";
    public const string EmptyOrb = "res://images/orbs/empty_orb.png";

    private static readonly Dictionary<string, Texture2D?> Cache = new();

    public static string ElementIcon(BotanistElement element) => element switch
    {
        BotanistElement.Earth => "res://botanist/images/elements/earth.png",
        BotanistElement.Fire => "res://botanist/images/elements/fire.png",
        BotanistElement.Water => "res://botanist/images/elements/water.png",
        BotanistElement.Wind => "res://botanist/images/elements/wind.png",
        BotanistElement.Aether => "res://botanist/images/elements/aether.png",
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
            // 预加载管理器可能主动卸载资源，静态缓存不能继续持有已释放的纹理。
            if (cached != null && GodotObject.IsInstanceValid(cached))
            {
                return cached;
            }

            Cache.Remove(path);
        }

        Texture2D? texture = ResourceLoader.Exists(path)
            ? ResourceLoader.Load<Texture2D>(path)
            : null;
        Cache[path] = texture;
        return texture;
    }
}
