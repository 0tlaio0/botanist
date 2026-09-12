using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace Botanist.Scripts;

[HarmonyPatch(typeof(ArchaicTooth), "GetTranscendenceStarterCard")]
public static class ArchaicToothBotanistStarterPatch
{
    [HarmonyPostfix]
    public static void Postfix(Player player, ref CardModel? __result)
    {
        if (__result != null)
        {
            return;
        }

        __result = player.Deck.Cards.FirstOrDefault(card => card is BotanistHumus);
    }
}

[HarmonyPatch(typeof(ArchaicTooth), "GetTranscendenceTransformedCard")]
public static class ArchaicToothBotanistTransformationPatch
{
    [HarmonyPostfix]
    public static void Postfix(CardModel starterCard, ref CardModel __result)
    {
        if (starterCard is not BotanistHumus)
        {
            return;
        }

        CardModel replacement = starterCard.Owner.RunState.CreateCard<BotanistLivingCulture>(starterCard.Owner);
        if (starterCard.IsUpgraded)
        {
            CardCmd.Upgrade(replacement);
        }

        if (starterCard.Enchantment is { } enchantment)
        {
            EnchantmentModel enchantmentClone = (EnchantmentModel)enchantment.MutableClone();
            CardCmd.Enchant(enchantmentClone, replacement, enchantmentClone.Amount);
        }

        __result = replacement;
    }
}

[HarmonyPatch(typeof(ArchaicTooth), nameof(ArchaicTooth.TranscendenceCards), MethodType.Getter)]
public static class ArchaicToothBotanistCardListPatch
{
    [HarmonyPostfix]
    public static void Postfix(ref List<CardModel> __result)
    {
        CardModel livingCulture = ModelDb.Card<BotanistLivingCulture>();
        if (!__result.Contains(livingCulture))
        {
            __result.Add(livingCulture);
        }
    }
}
