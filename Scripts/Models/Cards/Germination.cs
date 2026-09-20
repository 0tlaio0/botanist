// 中文卡名：催芽
// 卡面描述：你接下来打出的{BotanistGerminationPower:diff()}张[gold]种子[/gold]牌耗能变为0。
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
public class BotanistGermination : BotanistCardModel
{
    public override BotanistElement Element => BotanistElement.Water;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new PowerVar<BotanistGerminationPower>(1m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<BotanistGerminationPower>()];

    public BotanistGermination()
        : base(0, CardType.Skill, CardRarity.Common, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<BotanistGerminationPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["BotanistGerminationPower"].BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["BotanistGerminationPower"].UpgradeValueBy(1m);
    }
}
