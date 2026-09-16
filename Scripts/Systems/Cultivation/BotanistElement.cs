namespace Botanist.Scripts;

public enum BotanistElement
{
    None,
    Earth,
    Fire,
    Water,
    Wind,
    Aether
}

public static class BotanistElements
{
    public static string DisplayName(this BotanistElement element) => element switch
    {
        BotanistElement.Earth => "地",
        BotanistElement.Fire => "火",
        BotanistElement.Water => "水",
        BotanistElement.Wind => "风",
        BotanistElement.Aether => "以太",
        _ => string.Empty
    };

    public static string IconPath(this BotanistElement element) => BotanistArt.ElementIcon(element);
}
