using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Botanist.Scripts;

/// <summary>下一颗种子成长时额外结算一次成长效果。</summary>
public class BotanistBottledSeasonsPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string? CustomPackedIconPath => BotanistArt.Sprout;
    public override string? CustomBigIconPath => BotanistArt.Sprout;

    public async Task ConsumeExtraGrowth(PlayerChoiceContext choiceContext, IBotanistSeedCard seed)
    {
        Flash();
        await BotanistGrowthResolution.ResolveAsync(seed, choiceContext);
        await PowerCmd.Decrement(this);
    }
}
