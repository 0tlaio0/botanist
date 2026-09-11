using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;

namespace Qgs.Scripts;

/// <summary>初始遗物：每场战斗开始时将一张以太放入手牌。</summary>
[Pool(typeof(QgsRelicPool))]
public class QgsStarterRelic : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    public override string PackedIconPath => "res://qgs/images/qgs_character.svg";
    protected override string PackedIconOutlinePath => "res://qgs/images/qgs_character.svg";
    protected override string BigIconPath => "res://qgs/images/qgs_character.svg";

    public override async Task BeforeCombatStart()
    {
        Flash();
        var combatState = Owner.Creature.CombatState;
        if (combatState == null)
        {
            return;
        }

        QgsAether aether = combatState.CreateCard<QgsAether>(Owner);
        await CardPileCmd.AddGeneratedCardToCombat(aether, PileType.Hand, Owner);
    }
}

