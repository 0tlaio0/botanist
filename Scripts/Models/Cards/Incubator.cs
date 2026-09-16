// 中文卡名：恒温箱
// 卡面描述：每回合第一次打出[gold]火元素[/gold]牌时，获得{BotanistIncubatorPower:diff()}点能量。
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Botanist.Scripts;

[Pool(typeof(BotanistCardPool))]
public class BotanistIncubator : BotanistCardModel
{
    public override BotanistElement Element => BotanistElement.Fire;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new PowerVar<BotanistIncubatorPower>(1m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        base.EnergyHoverTip,
        HoverTipFactory.FromPower<BotanistIncubatorPower>()
    ];

    public BotanistIncubator()
        : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<BotanistIncubatorPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["BotanistIncubatorPower"].BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["BotanistIncubatorPower"].UpgradeValueBy(1m);
    }
}
