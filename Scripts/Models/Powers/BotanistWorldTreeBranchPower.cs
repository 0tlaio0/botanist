using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Botanist.Scripts;

public class BotanistWorldTreeBranchPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string? CustomPackedIconPath => "res://botanist/images/elements/wind.svg";
    public override string? CustomBigIconPath => "res://botanist/images/elements/wind.svg";

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player)
        {
            return;
        }

        ICombatState? combatState = player.Creature.CombatState;
        if (combatState == null)
        {
            return;
        }

        Flash();
        for (int i = 0; i < Amount; i++)
        {
            BotanistAether aether = combatState.CreateCard<BotanistAether>(player);
            await CardPileCmd.AddGeneratedCardToCombat(aether, PileType.Hand, player);
        }
    }
}
