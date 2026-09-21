// 中文卡名：磨刃
// 卡面描述：每当你生成[gold]插条[/gold]时，对随机敌人造成{BotanistSharpenPower:diff()}点伤害。
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
public class BotanistSharpen : BotanistCardModel
{
    public override BotanistElement Element => BotanistElement.Fire;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new PowerVar<BotanistSharpenPower>(4m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromCard<BotanistCutting>(),
        HoverTipFactory.FromPower<BotanistSharpenPower>()
    ];

    public BotanistSharpen()
        : base(1, CardType.Power, CardRarity.Rare, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<BotanistSharpenPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["BotanistSharpenPower"].BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["BotanistSharpenPower"].UpgradeValueBy(2m);
    }
}
