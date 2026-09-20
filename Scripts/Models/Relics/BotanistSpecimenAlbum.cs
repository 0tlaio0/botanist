using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Botanist.Scripts;

/// <summary>每场战斗首次种子成长时，将对应标本加入手牌。</summary>
[Pool(typeof(BotanistRelicPool))]
public class BotanistSpecimenAlbum : CustomRelicModel
{
    private bool _hasTriggeredThisCombat;

    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override string PackedIconPath => BotanistArt.Character;
    protected override string PackedIconOutlinePath => BotanistArt.Character;
    protected override string BigIconPath => BotanistArt.Character;

    public override Task BeforeCombatStart()
    {
        _hasTriggeredThisCombat = false;
        return Task.CompletedTask;
    }

    public async Task OnSeedCultivated(IBotanistSeedCard seed)
    {
        if (_hasTriggeredThisCombat || Owner.Creature.CombatState is not { } combatState)
        {
            return;
        }

        _hasTriggeredThisCombat = true;
        Flash();
        BotanistSpecimen specimen = combatState.CreateCard<BotanistSpecimen>(Owner);
        specimen.SetCopiedSeed(seed);
        await CardPileCmd.AddGeneratedCardToCombat(specimen, PileType.Hand, Owner);
    }
}
