using System.Collections.Generic;
using Godot;

namespace Qgs.Scripts;

public static class QgsArt
{
    private static readonly Dictionary<string, Texture2D?> Cache = new();

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
