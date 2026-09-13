// 中文卡名：野外调查
// 卡面描述：
// 获得{Block:diff()}点[gold]格挡[/gold]。
// 抽{Cards:diff()}张牌。
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Botanist.Scripts;

[Pool(typeof(BotanistCardPool))]
public class BotanistFieldSurvey : BotanistCardModel
{
    public override bool GainsBlock => true;
    public override BotanistElement Element => BotanistElement.Wind;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(1),
        new BlockVar(8, ValueProp.Move)
    ];

    public BotanistFieldSurvey() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1);
    }
}
