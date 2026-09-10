using Godot.Bridge;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;

namespace Qgs.Scripts;

[ModInitializer(nameof(Init))]
public class Entry
{
    public static void Init()
    {
        ScriptManagerBridge.LookupScriptsInAssembly(typeof(Entry).Assembly);
        new Harmony("qgs.cultivation").PatchAll();
    }
}
