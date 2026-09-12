// 中文卡名：天有四时
// 卡面描述：本回合内，你此前每使用过一种元素，这张牌就造成{Damage:diff()}点伤害一次。如果最终攻击次数为4或以上，则将攻击次数翻倍。
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Botanist.Scripts;

[Pool(typeof(BotanistCardPool))]
public class BotanistFourSeasonsOfHeaven : BotanistCardModel
{
    public override BotanistElement Element => BotanistElement.Aether;
    public override string PortraitPath => "res://botanist/images/botanist_character.svg";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(5, ValueProp.Move)];

    public BotanistFourSeasonsOfHeaven() : base(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        int hitCount = CombatManager.Instance.History.CardPlaysFinished
            .Where(entry =>
                entry.CardPlay.Card.Owner == Owner &&
                entry.HappenedThisTurn(CombatState))
            .Select(entry => entry.CardPlay.Card)
            .OfType<BotanistCardModel>()
            .Select(card => card.Element)
            .Where(element => element != BotanistElement.None)
            .Distinct()
            .Count();

        if (hitCount >= 4)
        {
            hitCount *= 2;
        }

        if (hitCount == 0)
        {
            return;
        }

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .WithHitCount(hitCount)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
    }
}
