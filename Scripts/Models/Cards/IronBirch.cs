// 中文卡名：坚铁桦
// 卡面描述：
// 获得{Block:diff()}点[gold]格挡[/gold]。
// [gold]成长[/gold]：获得{RipenBlock:diff()}点[gold]格挡[/gold]。
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
public class BotanistIronBirch : BotanistSeedCardModel
{
    public override bool GainsBlock => true;
    public override BotanistElement Element => BotanistElement.Wind;
    public override string PortraitPath => BotanistArt.IronBirch;

    public override string RipenSummary =>
        $"获得{DynamicVars["RipenBlock"].IntValue}点格挡";

    public override IReadOnlyList<KeyValuePair<BotanistElement, int>> Requirements =>
    [
        new(BotanistElement.Earth, 2),
        new(BotanistElement.Wind, 1)
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(8m, ValueProp.Move),
        new BlockVar("RipenBlock", 8m, ValueProp.Move)
    ];

    public BotanistIronBirch()
        : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
    }

    public override async Task OnRipen(PlayerChoiceContext choiceContext)
    {
        await CreatureCmd.GainBlock(
            Owner.Creature,
            DynamicVars["RipenBlock"].BaseValue,
            ValueProp.Move,
            null);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["RipenBlock"].UpgradeValueBy(4m);
    }
}
