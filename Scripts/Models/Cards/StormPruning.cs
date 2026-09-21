// 中文卡名：风暴修剪
// 卡面描述：本回合打出[gold]插条[/gold]时，对所有敌人造成{BotanistStormPruningPower:diff()}点伤害。
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
public class BotanistStormPruning : BotanistCardModel
{
    public override BotanistElement Element => BotanistElement.Wind;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new PowerVar<BotanistStormPruningPower>(3m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromCard<BotanistCutting>(),
        HoverTipFactory.FromPower<BotanistStormPruningPower>()
    ];

    public BotanistStormPruning()
        : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<BotanistStormPruningPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["BotanistStormPruningPower"].BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["BotanistStormPruningPower"].UpgradeValueBy(2m);
    }
}
