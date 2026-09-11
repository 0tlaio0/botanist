using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Qgs.Scripts;

/// <summary>由掌天瓶生成的培育加速牌。</summary>
[Pool(typeof(QgsCardPool))]
public class QgsCreationDew : QgsCardModel
{
    public override QgsElement Element => QgsElement.Aether;
    public override bool CanBeGeneratedInCombat => false;
    public override string PortraitPath => "res://qgs/images/qgs_character.svg";

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];

    public QgsCreationDew() : base(0, CardType.Skill, CardRarity.Token, TargetType.Self, false)
    {
    }

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) =>
        Task.CompletedTask;
}
