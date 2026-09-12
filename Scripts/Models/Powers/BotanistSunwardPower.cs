using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;

namespace Botanist.Scripts;

public class BotanistSunwardPower : CustomPowerModel
{
    private int _turnsUntilEnergy = 2;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string? CustomPackedIconPath => "res://botanist/images/powers/sunward.svg";
    public override string? CustomBigIconPath => "res://botanist/images/powers/sunward.svg";

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.ForEnergy(this)
    ];

    public override async Task AfterEnergyReset(Player player)
    {
        if (player != Owner.Player)
        {
            return;
        }

        _turnsUntilEnergy--;
        if (_turnsUntilEnergy > 0)
        {
            return;
        }

        Flash();
        await PlayerCmd.GainEnergy(Amount, player);
        _turnsUntilEnergy = 2;
    }
}
