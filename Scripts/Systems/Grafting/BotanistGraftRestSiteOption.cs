using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace Botanist.Scripts;

public sealed class BotanistGraftRestSiteOption : CustomRestSiteOption
{
    public const string Id = "BOTANIST_GRAFT";

    public override string OptionId => Id;
    public override string? CustomIconPath => BotanistArt.SeedlingSeed;
    public override IEnumerable<string> AssetPaths =>
        CustomIconPath is { } iconPath ? [iconPath] : base.AssetPaths;

    public override bool IsEnabled => HasValidPair(out _);

    public override LocString Description
    {
        get
        {
            _ = HasValidPair(out bool hasRoot);
            string key = IsEnabled
                ? $"OPTION_{Id}.enabledDescription"
                : hasRoot
                    ? $"OPTION_{Id}.noScionDescription"
                    : $"OPTION_{Id}.noRootDescription";
            return new LocString("rest_site_ui", key);
        }
    }

    public BotanistGraftRestSiteOption(Player owner) : base(owner)
    {
    }

    public override Task<bool> OnSelect()
    {
        return IsEnabled
            ? BotanistGraftScreen.RunAsync(Owner)
            : Task.FromResult(false);
    }

    private bool HasValidPair(out bool hasRoot)
    {
        hasRoot = false;
        foreach (CardModel root in PileType.Deck.GetPile(Owner).Cards)
        {
            if (!BotanistGraftService.CanBeGraftRoot(root))
            {
                continue;
            }

            hasRoot = true;
            if (PileType.Deck.GetPile(Owner).Cards.Any(
                    scion => BotanistGraftService.CanBeScion(scion) &&
                             BotanistGraftService.CanGraft(root, scion)))
            {
                return true;
            }
        }

        return false;
    }
}
