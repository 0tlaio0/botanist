using System.Collections.Generic;

namespace Qgs.Scripts;

public enum QgsElement
{
    None,
    Earth,
    Fire,
    Water,
    Wind,
    Aether
}

public static class QgsElements
{
    public static string DisplayName(this QgsElement element) => element switch
    {
        QgsElement.Earth => "土",
        QgsElement.Fire => "火",
        QgsElement.Water => "水",
        QgsElement.Wind => "风",
        QgsElement.Aether => "以太",
        _ => string.Empty
    };

    public static string IconPath(this QgsElement element) => element switch
    {
        QgsElement.Earth => "res://qgs/images/elements/earth.svg",
        QgsElement.Fire => "res://qgs/images/elements/fire.svg",
        QgsElement.Water => "res://qgs/images/elements/water.svg",
        QgsElement.Wind => "res://qgs/images/elements/wind.svg",
        QgsElement.Aether => "res://qgs/images/elements/aether.svg",
        _ => string.Empty
    };

    public static IEnumerable<QgsElement> IconElements(this QgsElement element)
    {
        if (element != QgsElement.None)
        {
            yield return element;
        }
    }
}
