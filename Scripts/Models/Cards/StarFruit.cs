// 中文卡名：星之果实
// 卡面描述：
// [gold]成长[/gold]：获得{StrengthPower:diff()}点[gold]力量[/gold]和{DexterityPower:diff()}点[gold]敏捷[/gold]。
// 战斗结束后，回复{Heal:diff()}点生命。
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
public class BotanistStarFruit : BotanistSeedCardModel
{
    public override BotanistElement Element => BotanistElement.Water;

    public override string RipenSummary =>
        $"获得{DynamicVars.Strength.IntValue}点力量与{DynamicVars.Dexterity.IntValue}点敏捷；" +
        $"战斗结束后回复{DynamicVars.Heal.IntValue}点生命";

    public override IReadOnlyList<KeyValuePair<BotanistElement, int>> Requirements =>
    [
        new(BotanistElement.Water, 2),
        new(BotanistElement.Earth, 2),
        new(BotanistElement.Fire, 2)
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<StrengthPower>(1m),
        new PowerVar<DexterityPower>(1m),
        new HealVar(6m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<DexterityPower>(),
        HoverTipFactory.FromPower<BotanistStarFruitPower>()
    ];

    public BotanistStarFruit() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    public override async Task OnRipen(PlayerChoiceContext choiceContext)
    {
        await PowerCmd.Apply<StrengthPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars.Strength.BaseValue,
            Owner.Creature,
            this);
        await PowerCmd.Apply<DexterityPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars.Dexterity.BaseValue,
            Owner.Creature,
            this);
        await PowerCmd.Apply<BotanistStarFruitPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars.Heal.BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Heal.UpgradeValueBy(3m);
    }
}
