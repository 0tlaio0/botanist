// 中文卡名：手套
// 卡面描述：获得{Block:diff()}点[gold]格挡[/gold]。本回合前2张[gold]插条[/gold]打出后抽1张牌。
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
public class BotanistGloves : BotanistCardModel
{
    private const decimal CuttingDraws = 2m;

    public override bool GainsBlock => true;
    public override BotanistElement Element => BotanistElement.Earth;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new BlockVar(6m, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromCard<BotanistCutting>(),
        HoverTipFactory.FromPower<BotanistGlovesPower>()
    ];

    public BotanistGloves()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        await PowerCmd.Apply<BotanistGlovesPower>(
            choiceContext,
            Owner.Creature,
            CuttingDraws,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3m);
    }
}
