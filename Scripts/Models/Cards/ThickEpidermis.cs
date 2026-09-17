// 中文卡名：厚实表皮
// 卡面描述：本回合受到的伤害减少25%。
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace Botanist.Scripts;

[Pool(typeof(BotanistCardPool))]
public class BotanistThickEpidermis : BotanistCardModel
{
    public override BotanistElement Element => BotanistElement.Earth;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<BotanistThickEpidermisPower>()];

    public BotanistThickEpidermis()
        : base(2, CardType.Skill, CardRarity.Rare, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<BotanistThickEpidermisPower>(
            choiceContext,
            Owner.Creature,
            1m,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
