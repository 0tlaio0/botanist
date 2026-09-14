// 中文卡名：噬元金藤
// 卡面描述：
// 夺取前一颗[gold]种子[/gold]将吸取的元素。
// [gold]成长[/gold]：给予敌人{Parasite:diff()}层[gold]寄生[/gold]。
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
public class BotanistDevouringGoldVine : BotanistTargetedSeedCardModel
{
    private const string ParasiteKey = "Parasite";

    public override BotanistElement Element => BotanistElement.Wind;
    public override bool StealsPreviousSeedGrowth => true;
    public override string PortraitPath => BotanistArt.DevouringGoldVine;

    public override string RipenSummary =>
        $"给予敌人{DynamicVars[ParasiteKey].IntValue}层寄生";

    public override IReadOnlyList<KeyValuePair<BotanistElement, int>> Requirements =>
    [
        new(BotanistElement.Fire, 3),
        new(BotanistElement.Water, 3),
        new(BotanistElement.Wind, 3)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        BotanistKeywords.Growth,
        CardKeyword.Exhaust
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new PowerVar<BotanistParasitePower>(ParasiteKey, 5m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<BotanistParasitePower>()];

    public BotanistDevouringGoldVine()
        : base(1, CardType.Skill, CardRarity.Rare, TargetType.AnyEnemy, true)
    {
    }

    protected override Task OnSow(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        return Task.CompletedTask;
    }

    public override async Task OnRipen(PlayerChoiceContext choiceContext)
    {
        if (RipenTarget is not { } target)
        {
            return;
        }

        await PowerCmd.Apply<BotanistParasitePower>(
            choiceContext,
            target,
            DynamicVars[ParasiteKey].BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[ParasiteKey].UpgradeValueBy(2m);
    }
}
