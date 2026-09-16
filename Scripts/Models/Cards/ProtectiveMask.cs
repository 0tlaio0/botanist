// 中文卡名：防护面罩
// 卡面描述：
// 获得{Block:diff()}点[gold]格挡[/gold]。
// 若有敌人[gold]中毒[/gold]，额外获得{ConditionalBlock:diff()}点[gold]格挡[/gold]。
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Botanist.Scripts;

[Pool(typeof(BotanistCardPool))]
public class BotanistProtectiveMask : BotanistCardModel
{
    public override bool GainsBlock => true;
    public override BotanistElement Element => BotanistElement.Earth;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(6m, ValueProp.Move),
        new DynamicVar("ConditionalBlock", 4m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<PoisonPower>()];

    public BotanistProtectiveMask()
        : base(1, CardType.Skill, CardRarity.Common, TargetType.Self, true)
    {
    }

    protected override bool ShouldGlowGoldInternal => HasPoisonedEnemy;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        decimal block = DynamicVars.Block.BaseValue;
        if (HasPoisonedEnemy)
        {
            block += DynamicVars["ConditionalBlock"].BaseValue;
        }

        await CreatureCmd.GainBlock(Owner.Creature, block, ValueProp.Move, cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3m);
        DynamicVars["ConditionalBlock"].UpgradeValueBy(2m);
    }

    private bool HasPoisonedEnemy =>
        CombatState?.HittableEnemies.Any(enemy => enemy.GetPowerAmount<PoisonPower>() > 0) ?? false;
}
