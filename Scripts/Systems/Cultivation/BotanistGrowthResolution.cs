using System.Threading;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Botanist.Scripts;

/// <summary>标记当前异步结算是否来自成长效果，供伤害修正能力判断来源。</summary>
internal static class BotanistGrowthResolution
{
    private static readonly AsyncLocal<int> ResolutionDepth = new();

    public static bool IsResolving => ResolutionDepth.Value > 0;

    public static async Task ResolveAsync(
        IBotanistSeedCard seed,
        PlayerChoiceContext choiceContext)
    {
        ResolutionDepth.Value++;
        try
        {
            await seed.OnRipen(choiceContext);
        }
        finally
        {
            ResolutionDepth.Value--;
        }
    }
}
