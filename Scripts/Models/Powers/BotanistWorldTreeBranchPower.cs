using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Botanist.Scripts;

/// <summary>每回合开始时按层数将以太加入手牌。</summary>
public class BotanistWorldTreeBranchPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string? CustomPackedIconPath => BotanistArt.ElementIcon(BotanistElement.Aether);
    public override string? CustomBigIconPath => BotanistArt.ElementIcon(BotanistElement.Aether);

    public override async Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext,
        MegaCrit.Sts2.Core.Entities.Players.Player player)
    {
        if (player != Owner.Player)
        {
            return;
        }

        var combatState = CombatState;
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
