using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace Botanist.Scripts;

/// <summary>异界烙印的先古升级版：每场战斗开始时加入两张以太+。</summary>
[Pool(typeof(BotanistRelicPool))]
public class BotanistWisteriaWreath : CustomRelicModel
{
    private const int AetherCount = 2;

    public override RelicRarity Rarity => RelicRarity.Starter;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new CardsVar(AetherCount)];

    public override string PackedIconPath => BotanistArt.Character;
    protected override string PackedIconOutlinePath => BotanistArt.Character;
    protected override string BigIconPath => BotanistArt.Character;

    public override async Task BeforeCombatStart()
    {
        ICombatState? combatState = Owner.Creature.CombatState;
        if (combatState == null)
        {
            return;
        }

        Flash();
        for (int index = 0; index < AetherCount; index++)
        {
            BotanistAether aether = combatState.CreateCard<BotanistAether>(Owner);
            CardCmd.Upgrade(aether, CardPreviewStyle.None);
            await CardPileCmd.AddGeneratedCardToCombat(aether, PileType.Hand, Owner);
        }
    }
}
