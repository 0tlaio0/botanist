// 中文卡名：向日葵
// 卡面描述：
// 获得{Block:diff()}点[gold]格挡[/gold]。
// [gold]成长[/gold]：获得一层[gold]向阳[/gold]。
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
public class BotanistSunflower : BotanistSeedCardModel
{
    public override bool GainsBlock => true;
    public override BotanistElement Element => BotanistElement.Fire;
    public override string PortraitPath => "res://botanist/images/botanist_character.svg";
    public override string RipenSummary => "获得1层向阳";

    public override IReadOnlyList<KeyValuePair<BotanistElement, int>> Requirements =>
    [
        new(BotanistElement.Fire, 2),
        new(BotanistElement.Earth, 1)
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(3, ValueProp.Move)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<BotanistSunwardPower>()
    ];

    public BotanistSunflower() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
    }

    public override async Task OnRipen(PlayerChoiceContext choiceContext)
    {
        await PowerCmd.Apply<BotanistSunwardPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(2);
    }
}
