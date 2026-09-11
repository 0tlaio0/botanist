// 中文卡名：世界树枝条
// 卡面描述：在你的回合开始时，在你的手牌中加入1张[gold]以太[/gold]。
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace Qgs.Scripts;

[Pool(typeof(QgsCardPool))]
public class QgsWorldTreeBranch : QgsCardModel
{
    public override QgsElement Element => QgsElement.Wind;
    public override string PortraitPath => "res://qgs/images/qgs_character.svg";

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromCard<QgsAether>()];

    public QgsWorldTreeBranch() : base(3, CardType.Power, CardRarity.Ancient, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<QgsWorldTreeBranchPower>(
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
