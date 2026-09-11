// 中文卡名：园艺手套
// 卡面描述：获得{DexterityPower:diff()}点敏捷。
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Qgs.Scripts;

[Pool(typeof(QgsCardPool))]
public class QgsGloves : QgsCardModel
{
    public override QgsElement Element => QgsElement.Earth;
    public override string PortraitPath => "res://qgs/images/qgs_character.svg";

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<DexterityPower>()];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new PowerVar<DexterityPower>(2m)];

    public QgsGloves() : base(1, CardType.Power, CardRarity.Common, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<DexterityPower>(choiceContext, Owner.Creature, DynamicVars.Dexterity.BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Dexterity.UpgradeValueBy(1);
    }
}
