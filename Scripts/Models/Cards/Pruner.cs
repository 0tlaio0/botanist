// 中文卡名：剪枝刀
// 卡面描述：造成{Damage:diff()}点伤害。
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Qgs.Scripts;

[Pool(typeof(QgsCardPool))]
public class QgsPruner : QgsCardModel
{
    public override QgsElement Element => QgsElement.Fire;
    public override string PortraitPath => "res://qgs/images/qgs_character.svg";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(9, ValueProp.Move)];

    public QgsPruner() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
    }
}
