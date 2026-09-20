// 中文卡名：火绒草
// 卡面描述：
// 获得{Block:diff()}点[gold]格挡[/gold]。
// [gold]成长[/gold]：造成{Damage:diff()}点伤害，抽{Cards:diff()}张牌。
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
public class BotanistEdelweiss : BotanistTargetedSeedCardModel
{
    public override bool GainsBlock => true;
    public override BotanistElement Element => BotanistElement.Fire;
    public override string PortraitPath => BotanistArt.Edelweiss;

    public override string RipenSummary =>
        $"造成{DynamicVars.Damage.IntValue}点伤害，抽{DynamicVars.Cards.IntValue}张牌";

    public override IReadOnlyList<KeyValuePair<BotanistElement, int>> Requirements =>
    [
        new(BotanistElement.Fire, 1),
        new(BotanistElement.Earth, 1)
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(5m, ValueProp.Move),
        new DamageVar(6m, ValueProp.Move),
        new CardsVar(1)
    ];

    public BotanistEdelweiss() : base(1, CardType.Skill, CardRarity.Basic, TargetType.AnyEnemy, true)
    {
    }

    protected override async Task OnSow(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
    }

    public override async Task OnRipen(PlayerChoiceContext choiceContext)
    {
        if (RipenTarget is { } target)
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this)
                .Targeting(target)
                .Execute(choiceContext);
        }
        else if (RipenCombatState is { } combatState && combatState.HittableEnemies.Count > 0)
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this)
                .TargetingRandomOpponents(combatState)
                .Execute(choiceContext);
        }

        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
    }
}
