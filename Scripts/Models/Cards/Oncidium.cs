// 中文卡名：文心兰
// 卡面描述：
// 获得{Block:diff()}点[gold]格挡[/gold]。
// [gold]成长[/gold]：抽{Cards:diff()}张牌。
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
public class BotanistOncidium : BotanistSeedCardModel
{
    public override bool GainsBlock => true;
    public override BotanistElement Element => BotanistElement.Water;
    public override string PortraitPath => BotanistArt.Oncidium;

    public override string RipenSummary =>
        $"抽{DynamicVars.Cards.IntValue}张牌";

    public override IReadOnlyList<KeyValuePair<BotanistElement, int>> Requirements =>
    [
        new(BotanistElement.Water, 2),
        new(BotanistElement.Earth, 1)
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(10m, ValueProp.Move),
        new CardsVar(2)
    ];

    public BotanistOncidium()
        : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
    }

    public override async Task OnRipen(PlayerChoiceContext choiceContext)
    {
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(5m);
    }
}
