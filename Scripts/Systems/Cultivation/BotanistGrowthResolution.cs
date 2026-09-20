using System.Threading;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Botanist.Scripts;

/// <summary>标记当前异步结算是否来自成长效果，供伤害修正能力判断来源。</summary>
internal static class BotanistGrowthResolution
{
    private static readonly AsyncLocal<int> ResolutionDepth = new();
    private static readonly AsyncLocal<TargetOverride?> TargetOverrides = new();

    public static bool IsResolving => ResolutionDepth.Value > 0;

    public static async Task ResolveAsync(
        IBotanistSeedCard seed,
        PlayerChoiceContext choiceContext)
    {
        ResolutionDepth.Value++;
        try
        {
            Creature? graftTarget = ResolveGraftTarget(seed);
            await seed.OnRipen(choiceContext);

            foreach (BotanistGraftSnapshot graft in BotanistGraftService.GetGraftSnapshots(seed.Card))
            {
                CardModel? scionCard = BotanistGraftService.CreateScionCard(seed.Card, graft);
                if (scionCard?.AsSeed() is not { } scion)
                {
                    continue;
                }

                Creature? target = graftTarget;
                if (scionCard.TargetType == TargetType.AnyEnemy &&
                    !IsValidGraftTarget(seed.Card, target))
                {
                    target = RandomLivingEnemy(seed.Card);
                }

                using IDisposable _
                    = PushTargetOverride(target);
                await scion.OnRipen(choiceContext);
            }
        }
        finally
        {
            ResolutionDepth.Value--;
        }
    }

    internal static bool TryGetTargetOverride(out Creature? target)
    {
        TargetOverride? current = TargetOverrides.Value;
        target = current?.Target;
        return current != null;
    }

    private static IDisposable PushTargetOverride(Creature? target)
    {
        TargetOverride? previous = TargetOverrides.Value;
        TargetOverrides.Value = new TargetOverride(target);
        return new TargetOverrideRegistration(previous);
    }

    private static Creature? ResolveGraftTarget(IBotanistSeedCard seed)
    {
        Creature? target = seed.Card.CurrentTarget;
        if (IsValidGraftTarget(seed.Card, target))
        {
            return target;
        }

        Creature? ownTarget = (seed as BotanistTargetedSeedCardModel)?.OwnRipenTarget;
        return IsValidGraftTarget(seed.Card, ownTarget) ? ownTarget : null;
    }

    private static bool IsValidGraftTarget(CardModel root, Creature? target)
    {
        return target is { IsAlive: true } &&
               root.CombatState is { } combatState &&
               !combatState.EscapedCreatures.Contains(target) &&
               combatState.HittableEnemies.Contains(target);
    }

    private static Creature? RandomLivingEnemy(CardModel root)
    {
        return root.Owner.Creature.CombatState is { } combatState
            ? root.Owner.RunState.Rng.CombatTargets.NextItem(combatState.HittableEnemies)
            : null;
    }

    private sealed record TargetOverride(Creature? Target);

    private sealed class TargetOverrideRegistration(TargetOverride? previous) : IDisposable
    {
        public void Dispose()
        {
            TargetOverrides.Value = previous;
        }
    }
}
