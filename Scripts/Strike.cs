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
public class QgsStrike : QgsCardModel
{
    public override QgsElement Element => QgsElement.Fire;
    public override IEnumerable<CardTag> Tags => [CardTag.Strike];
    public override string PortraitPath => "res://qgs/images/qgs_character.svg";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [new DamageVar(6, ValueProp.Move)];

    public QgsStrike() : base(1, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy, true)
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
