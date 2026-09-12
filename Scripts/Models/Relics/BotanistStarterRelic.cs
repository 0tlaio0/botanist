using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;

namespace Botanist.Scripts;

/// <summary>初始遗物：每场战斗开始时将一张以太放入手牌。</summary>
[Pool(typeof(BotanistRelicPool))]
public class BotanistStarterRelic : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    public override string PackedIconPath => "res://botanist/images/botanist_character.svg";
    protected override string PackedIconOutlinePath => "res://botanist/images/botanist_character.svg";
    protected override string BigIconPath => "res://botanist/images/botanist_character.svg";

    public override async Task BeforeCombatStart()
    {
        Flash();
        var combatState = Owner.Creature.CombatState;
        if (combatState == null)
        {
            return;
        }

        BotanistAether aether = combatState.CreateCard<BotanistAether>(Owner);
        await CardPileCmd.AddGeneratedCardToCombat(aether, PileType.Hand, Owner);
    }
}

