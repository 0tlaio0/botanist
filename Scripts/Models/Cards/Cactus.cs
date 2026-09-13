// 中文卡名：仙人掌
// 卡面描述：
// 获得{Block:diff()}点[gold]格挡[/gold]。
// [gold]成长[/gold]：获得{ThornsPower:diff()}层[gold]荆棘[/gold]。
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Botanist.Scripts;

[Pool(typeof(BotanistCardPool))]
public class BotanistCactus : BotanistSeedCardModel
{
    public override bool GainsBlock => true;
    public override BotanistElement Element => BotanistElement.Earth;

    public override string RipenSummary =>
        $"获得{DynamicVars["ThornsPower"].IntValue}层荆棘";

    public override IReadOnlyList<KeyValuePair<BotanistElement, int>> Requirements =>
    [
        new(BotanistElement.Earth, 2),
        new(BotanistElement.Fire, 1)
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(5m, ValueProp.Move),
        new PowerVar<ThornsPower>(3m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<ThornsPower>()];

    public BotanistCactus() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
    }

    public override async Task OnRipen(PlayerChoiceContext choiceContext)
    {
        await PowerCmd.Apply<ThornsPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["ThornsPower"].BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["ThornsPower"].UpgradeValueBy(1m);
    }
}
