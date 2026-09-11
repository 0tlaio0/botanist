// 中文卡名：土壤开掘
// 卡面描述：获得{Capacity:diff()}个培育区栏位。
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Qgs.Scripts;

[Pool(typeof(QgsCardPool))]
public class QgsSoilExcavation : QgsCardModel
{
    public override QgsElement Element => QgsElement.Earth;
    public override string PortraitPath => "res://qgs/images/qgs_character.svg";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new IntVar("Capacity", 2m)];

    public QgsSoilExcavation() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        QgsCultivation.AddCapacity(Owner, DynamicVars["Capacity"].IntValue);
        return Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Capacity"].UpgradeValueBy(1);
    }
}
