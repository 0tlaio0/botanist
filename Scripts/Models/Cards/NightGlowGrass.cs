// 中文卡名：夜光草
// 卡面描述：
// [gold]预见[/gold]{Scry:diff()}。
// [gold]成长[/gold]：抽{Cards:diff()}张牌。
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Cards.Variables;
using BaseLib.Commands;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Botanist.Scripts;

[Pool(typeof(BotanistCardPool))]
public class BotanistNightGlowGrass : BotanistSeedCardModel
{
    public override BotanistElement Element => BotanistElement.Wind;
    public override string PortraitPath => BotanistArt.NightGlowGrass;

    public override string RipenSummary =>
        $"抽{DynamicVars.Cards.IntValue}张牌";

    public override IReadOnlyList<KeyValuePair<BotanistElement, int>> Requirements =>
    [
        new(BotanistElement.Wind, 2),
        new(BotanistElement.Water, 2)
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new ScryVar(3),
        new CardsVar(2)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.Static(BaseLibTip.Scry, DynamicVars["Scry"])];

    public BotanistNightGlowGrass()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        await ScryCmd.Execute(choiceContext, this);
    }

    public override async Task OnRipen(PlayerChoiceContext choiceContext)
    {
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1m);
    }
}
