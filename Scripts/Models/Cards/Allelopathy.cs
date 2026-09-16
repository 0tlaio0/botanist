// 中文卡名：化感作用
// 卡面描述：每当植物成熟时，对所有敌人施加{BotanistAllelopathyPower:diff()}层[gold]中毒[/gold]。
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
public class BotanistAllelopathy : BotanistCardModel
{
    public override BotanistElement Element => BotanistElement.Fire;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new PowerVar<BotanistAllelopathyPower>(4m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<BotanistAllelopathyPower>(),
        HoverTipFactory.FromPower<PoisonPower>()
    ];

    public BotanistAllelopathy()
        : base(1, CardType.Power, CardRarity.Rare, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<BotanistAllelopathyPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["BotanistAllelopathyPower"].BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["BotanistAllelopathyPower"].UpgradeValueBy(2m);
    }
}
