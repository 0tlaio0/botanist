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

namespace Botanist.Scripts;

/// <summary>本场下一颗种子成长时额外结算一次成长效果。</summary>
[Pool(typeof(BotanistPotionPool))]
public class BotanistBottledSeasons : CustomPotionModel
{
    public override PotionRarity Rarity => PotionRarity.Rare;
    public override PotionUsage Usage => PotionUsage.CombatOnly;
    public override TargetType TargetType => TargetType.Self;
    public override string? CustomPackedImagePath => BotanistArt.Sprout;
    public override string? CustomPackedOutlinePath => BotanistArt.Sprout;

    public override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromKeyword(BotanistKeywords.Growth)];

    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        await PowerCmd.Apply<BotanistBottledSeasonsPower>(
            choiceContext,
            Owner.Creature,
            1m,
            Owner.Creature,
            null);
    }
}
