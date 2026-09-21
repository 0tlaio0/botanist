using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Botanist.Scripts;

/// <summary>获得格挡，培养区每有一颗种子再获得格挡。</summary>
[Pool(typeof(BotanistPotionPool))]
public class BotanistChlorophyll : CustomPotionModel
{
    public override PotionRarity Rarity => PotionRarity.Common;
    public override PotionUsage Usage => PotionUsage.CombatOnly;
    public override TargetType TargetType => TargetType.Self;
    public override string? CustomPackedImagePath => BotanistArt.Sprout;
    public override string? CustomPackedOutlinePath => BotanistArt.Sprout;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(10m, ValueProp.Unpowered),
        new DynamicVar("BonusBlock", 3m)
    ];

    public override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.Static(StaticHoverTip.Block)];

    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        int seedCount = BotanistCultivation.GetPlanted(Owner).Count;
        decimal block = DynamicVars.Block.BaseValue + DynamicVars["BonusBlock"].BaseValue * seedCount;
        await CreatureCmd.GainBlock(Owner.Creature, block, ValueProp.Unpowered, null);
    }
}
