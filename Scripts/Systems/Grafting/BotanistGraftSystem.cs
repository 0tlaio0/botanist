using System.Collections.Generic;
using System.Linq;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;

namespace Botanist.Scripts;

public sealed class BotanistGraftSystem : CustomSingletonModel
{
    public BotanistGraftSystem() : base(HookType.Run)
    {
    }

    public override bool TryModifyRestSiteOptions(
        Player player,
        ICollection<RestSiteOption> options)
    {
        if (player.Character is not BotanistCharacter ||
            options.Any(option => option.OptionId == BotanistGraftRestSiteOption.Id))
        {
            return false;
        }

        options.Add(new BotanistGraftRestSiteOption(player));
        return true;
    }
}
