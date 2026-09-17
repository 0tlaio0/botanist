// 中文卡名：繁根加护
// 卡面描述：每当你打出一张[gold]种子[/gold]牌，获得{Block:diff()}点[gold]格挡[/gold]。
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Botanist.Scripts;

[Pool(typeof(BotanistCardPool))]
public class BotanistRootingBlessing : BotanistCardModel
{
    public override BotanistElement Element => BotanistElement.Earth;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new BlockVar(4m, ValueProp.Unpowered)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.Static(StaticHoverTip.Block)];

    public BotanistRootingBlessing()
        : base(2, CardType.Power, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<BotanistRootingBlessingPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars.Block.BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
        DynamicVars.Block.UpgradeValueBy(2m);
    }
}
