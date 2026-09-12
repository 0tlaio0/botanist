using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Botanist.Scripts;

/// <summary>记录每张种子第一次成熟，并降低该种子在本场战斗的耗能。</summary>
public class BotanistSeedBankPower : CustomPowerModel
{
    private readonly HashSet<CardModel> _reducedSeeds = [];

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string? CustomPackedIconPath => BotanistArt.Sprout;
    public override string? CustomBigIconPath => BotanistArt.Sprout;

    public Task OnSeedCultivated(PlayerChoiceContext choiceContext, CardModel seed)
    {
        if (seed.Owner != Owner.Player || !_reducedSeeds.Add(seed))
        {
            return Task.CompletedTask;
        }

        seed.EnergyCost.AddThisCombat(-Amount, reduceOnly: true);
        Flash();
        return Task.CompletedTask;
    }
}
