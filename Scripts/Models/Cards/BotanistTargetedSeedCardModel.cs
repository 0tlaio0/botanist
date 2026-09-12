using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Botanist.Scripts;

/// <summary>记录播种目标，供需要在成熟时继续攻击同一目标的种子牌使用。</summary>
public abstract class BotanistTargetedSeedCardModel : BotanistSeedCardModel
{
    private Creature? _playTarget;

    protected Creature SowTarget => _playTarget!;

    protected Creature? RipenTarget =>
        _playTarget is { IsAlive: true } ? _playTarget : null;

    protected BotanistTargetedSeedCardModel(
        int energyCost,
        CardType cardType,
        CardRarity rarity,
        TargetType targetType,
        bool shouldShowInCardLibrary)
        : base(energyCost, cardType, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected sealed override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        _playTarget = cardPlay.Target;
        await OnSow(choiceContext, cardPlay);
    }

    protected abstract Task OnSow(PlayerChoiceContext choiceContext, CardPlay cardPlay);
}
