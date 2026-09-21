// 中文卡名：繁根加护
// 卡面描述：
// 每当你打出一张[gold]种子[/gold]牌，获得{Block:diff()}点[gold]格挡[/gold]。
// 回合结束时，培养区中每有1颗[gold]种子[/gold]，获得{EndBlock:diff()}点[gold]格挡[/gold]。
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
    [
        new BlockVar(4m, ValueProp.Unpowered),
        new DynamicVar("EndBlock", 3m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.Static(StaticHoverTip.Block)];

    public BotanistRootingBlessing()
        : base(2, CardType.Power, CardRarity.Rare, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        decimal endBlock = DynamicVars["EndBlock"].BaseValue;
        BotanistRootingBlessingPower? existing =
            Owner.Creature.GetPower<BotanistRootingBlessingPower>();

        await PowerCmd.Apply<BotanistRootingBlessingPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars.Block.BaseValue,
            Owner.Creature,
            this);

        BotanistRootingBlessingPower? power =
            Owner.Creature.GetPower<BotanistRootingBlessingPower>();
        if (power == null)
        {
            return;
        }

        // Amount 由能力叠层处理；结束格挡是额外字段，需要按打出次数自己累加。
        if (existing == null)
        {
            power.SetEndBlock(endBlock);
        }
        else
        {
            power.AddEndBlock(endBlock);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
        DynamicVars.Block.UpgradeValueBy(2m);
        DynamicVars["EndBlock"].UpgradeValueBy(1m);
    }
}