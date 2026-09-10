using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Qgs.Scripts;

/// <summary>初始遗物：每场战斗开始时抽 1 张牌。</summary>
[Pool(typeof(QgsRelicPool))]
public class QgsStarterRelic : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [new CardsVar(1)];

    public override string PackedIconPath => "res://qgs/images/qgs_character.svg";
    protected override string PackedIconOutlinePath => "res://qgs/images/qgs_character.svg";
    protected override string BigIconPath => "res://qgs/images/qgs_character.svg";

    public override async Task BeforeCombatStart()
    {
        Flash();
        await CardPileCmd.Draw(new ThrowingPlayerChoiceContext(), DynamicVars.Cards.IntValue, Owner);
    }
}

