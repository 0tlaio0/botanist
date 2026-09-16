using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace Botanist.Scripts;

/// <summary>在下个玩家回合抽牌前杀死持有者。</summary>
public class BotanistColchicinePower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override string? CustomPackedIconPath => BotanistArt.ElementIcon(BotanistElement.Water);
    public override string? CustomBigIconPath => BotanistArt.ElementIcon(BotanistElement.Water);

    public override async Task BeforeHandDraw(
        Player player,
        PlayerChoiceContext choiceContext,
        ICombatState combatState)
    {
        if (player != Owner.Player)
        {
            return;
        }

        await CreatureCmd.Damage(choiceContext, Owner, 99999m, ValueProp.Unpowered, Owner);
        Flash();
        await PowerCmd.TickDownDuration(this);
    }
}
