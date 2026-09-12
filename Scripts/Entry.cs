using Godot.Bridge;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;

namespace Botanist.Scripts;

[ModInitializer(nameof(Init))]
public class Entry
{
	public static void Init()
	{
		ScriptManagerBridge.LookupScriptsInAssembly(typeof(Entry).Assembly);
		new Harmony("botanist.cultivation").PatchAll();
	}
}
