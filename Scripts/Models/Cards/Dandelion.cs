// 中文卡名：蒲公英
// 卡面描述：
// 抽{Cards:diff()}张牌。
// [gold]成长[/gold]：随机对敌人造成{Damage:diff()}点伤害，将一张此牌的复制品加入你的弃牌堆。
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Botanist.Scripts;

[Pool(typeof(BotanistCardPool))]
public class BotanistDandelion : BotanistSeedCardModel
{
    public override BotanistElement Element => BotanistElement.Wind;

    public override string RipenSummary =>
        $"随机对敌人造成{DynamicVars.Damage.IntValue}点伤害，将一张此牌的复制品加入弃牌堆";

    public override IReadOnlyList<KeyValuePair<BotanistElement, int>> Requirements =>
    [
        new(BotanistElement.Wind, 2)
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(1),
        new DamageVar(6m, ValueProp.Move)
    ];

    public BotanistDandelion() : base(0, CardType.Attack, CardRarity.Common, TargetType.RandomEnemy, true)
    {
    }

    public override async Task OnRipen(PlayerChoiceContext choiceContext)
    {
        if (CombatState is not { } combatState)
        {
            return;
        }

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .TargetingRandomOpponents(combatState)
            .Execute(choiceContext);

        CardModel copy = CreateClone();
        CardCmd.PreviewCardPileAdd(
            await CardPileCmd.AddGeneratedCardToCombat(copy, PileType.Discard, Owner),
            2.2f);
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
    }
}
