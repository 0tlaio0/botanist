// 中文卡名：汲露
// 卡面描述：培养区中每有1颗[gold]种子[/gold]，获得{Block:diff()}点[gold]格挡[/gold]。
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
public class BotanistCollectDew : BotanistCardModel
{
    public override bool GainsBlock => true;
    public override BotanistElement Element => BotanistElement.Water;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new BlockVar(4, ValueProp.Move)];

    public BotanistCollectDew() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int seedCount = BotanistCultivation.GetPlanted(Owner).Count;
        for (int i = 0; i < seedCount; i++)
        {
            await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(1);
    }
}
