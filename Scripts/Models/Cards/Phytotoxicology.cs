// 中文卡名：植物毒理学
// 卡面描述：
// 每次因植物[gold]成长[/gold]效果给予[gold]中毒[/gold]时，所给予的层数增加{BotanistPhytotoxicologyPower:diff()}。
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Botanist.Scripts;

[Pool(typeof(BotanistCardPool))]
public class BotanistPhytotoxicology : BotanistCardModel
{
    public override BotanistElement Element => BotanistElement.Wind;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new PowerVar<BotanistPhytotoxicologyPower>(1m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<BotanistPhytotoxicologyPower>(),
        HoverTipFactory.FromPower<PoisonPower>()
    ];

    public BotanistPhytotoxicology()
        : base(1, CardType.Power, CardRarity.Rare, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<BotanistPhytotoxicologyPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["BotanistPhytotoxicologyPower"].BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["BotanistPhytotoxicologyPower"].UpgradeValueBy(1m);
    }
}
