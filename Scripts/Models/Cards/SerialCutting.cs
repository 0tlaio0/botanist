// 中文卡名：连插
// 卡面描述：你打出的下{BotanistSerialCuttingPower:diff()}张攻击牌会将1张同元素[gold]插条[/gold]加入你的手牌。
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
public class BotanistSerialCutting : BotanistCardModel
{
    public override BotanistElement Element => BotanistElement.Wind;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new PowerVar<BotanistSerialCuttingPower>(1m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromCard<BotanistCutting>(),
        HoverTipFactory.FromPower<BotanistSerialCuttingPower>()
    ];

    public BotanistSerialCutting()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<BotanistSerialCuttingPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["BotanistSerialCuttingPower"].BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["BotanistSerialCuttingPower"].UpgradeValueBy(1m);
    }
}
