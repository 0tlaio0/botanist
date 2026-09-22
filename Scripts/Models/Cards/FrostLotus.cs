// 中文卡名：冰霜莲
// 卡面描述：
// 给予敌人{WeakPower:diff()}层[gold]虚弱[/gold]。
// [gold]成长[/gold]：给予敌人1层[gold]缓慢[/gold]。
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
public class BotanistFrostLotus : BotanistTargetedSeedCardModel
{
    public override BotanistElement Element => BotanistElement.Water;
    public override string PortraitPath => BotanistArt.FrostLotus;
    public override string RipenSummary => "给予敌人1层缓慢";

    public override IReadOnlyList<KeyValuePair<BotanistElement, int>> Requirements =>
    [
        new(BotanistElement.Water, 2),
        new(BotanistElement.Wind, 2)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        BotanistKeywords.Growth,
        CardKeyword.Exhaust
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new PowerVar<WeakPower>(1m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<SlowPower>()
    ];

    public BotanistFrostLotus()
        : base(0, CardType.Skill, CardRarity.Rare, TargetType.AnyEnemy, true)
    {
    }

    protected override async Task OnSow(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<WeakPower>(
            choiceContext,
            SowTarget,
            DynamicVars.Weak.BaseValue,
            Owner.Creature,
            this);
    }

    public override async Task OnRipen(PlayerChoiceContext choiceContext)
    {
        if (RipenTarget is not { } target)
        {
            return;
        }

        await PowerCmd.Apply<SlowPower>(
            choiceContext,
            target,
            1m,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Weak.UpgradeValueBy(1m);
    }
}
