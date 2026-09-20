using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Botanist.Scripts;

/// <summary>每场战斗首次打出种子时获得格挡。</summary>
[Pool(typeof(BotanistRelicPool))]
public class BotanistTagCard : CustomRelicModel
{
    private bool _hasTriggeredThisCombat;

    public override RelicRarity Rarity => RelicRarity.Common;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new BlockVar(6m, ValueProp.Unpowered)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.Static(StaticHoverTip.Block)];

    public override string PackedIconPath => BotanistArt.Character;
    protected override string PackedIconOutlinePath => BotanistArt.Character;
    protected override string BigIconPath => BotanistArt.Character;

    public override Task BeforeCombatStart()
    {
        _hasTriggeredThisCombat = false;
        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (_hasTriggeredThisCombat ||
            cardPlay.Card.Owner != Owner ||
            !cardPlay.Card.IsSeed())
        {
            return;
        }

        _hasTriggeredThisCombat = true;
        Flash();
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, null);
    }
}
