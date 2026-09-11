// 中文卡名：以太
// 卡面描述：培养区中所有种子的每种所需元素计数各减少1。
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Qgs.Scripts;

[Pool(typeof(QgsCardPool))]
public class QgsAether : QgsCardModel
{
    public override QgsElement Element => QgsElement.Aether;
    public override string PortraitPath => "res://qgs/images/qgs_character.svg";

    public QgsAether() : base(2, CardType.Skill, CardRarity.Basic, TargetType.Self, true)
    {
    }

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        return Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
