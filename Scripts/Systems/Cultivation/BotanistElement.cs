using System.Collections.Generic;

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
        BotanistElement.Earth => "土",
        BotanistElement.Fire => "火",
        BotanistElement.Water => "水",
        BotanistElement.Wind => "风",
        BotanistElement.Aether => "以太",
        _ => string.Empty
    };

    public static string IconPath(this BotanistElement element) => element switch
    {
        BotanistElement.Earth => "res://botanist/images/elements/earth.svg",
        BotanistElement.Fire => "res://botanist/images/elements/fire.svg",
        BotanistElement.Water => "res://botanist/images/elements/water.svg",
        BotanistElement.Wind => "res://botanist/images/elements/wind.svg",
        BotanistElement.Aether => "res://botanist/images/elements/aether.svg",
        _ => string.Empty
    };

    public static IEnumerable<BotanistElement> IconElements(this BotanistElement element)
    {
        if (element != BotanistElement.None)
        {
            yield return element;
        }
    }
}
