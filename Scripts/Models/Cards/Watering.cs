// 中文卡名：浇灌
// 卡面描述：抽{Cards:diff()}张牌。
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Qgs.Scripts;

[Pool(typeof(QgsCardPool))]
public class QgsWatering : QgsCardModel
{
    public override QgsElement Element => QgsElement.Water;
    public override string PortraitPath => "res://qgs/images/qgs_character.svg";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new CardsVar(2)];

    public QgsWatering() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1);
    }
}
