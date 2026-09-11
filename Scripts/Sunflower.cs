using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Qgs.Scripts;

[Pool(typeof(QgsCardPool))]
public class QgsSunflower : QgsSeedCardModel
{
    public override bool GainsBlock => true;
    public override QgsElement Element => QgsElement.Fire;
    public override string PortraitPath => "res://qgs/images/qgs_character.svg";
    public override string RipenSummary => "获得1层向阳";

    public override IReadOnlyList<KeyValuePair<QgsElement, int>> Requirements =>
    [
        new(QgsElement.Fire, 2),
        new(QgsElement.Earth, 1)
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(3, ValueProp.Move)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<QgsSunwardPower>()
    ];

    public QgsSunflower() : base(1, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
    }

    public override async Task OnRipen(PlayerChoiceContext choiceContext)
    {
        await PowerCmd.Apply<QgsSunwardPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(2);
    }
}
