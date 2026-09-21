// 中文卡名：燎芽
// 卡面描述：造成{Damage:diff()}点伤害。若本场战斗打出的上一张牌是[gold]种子[/gold]牌或[gold]插条[/gold]，额外造成{BonusDamage:diff()}点伤害。
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
public class BotanistBlazingBud : BotanistCardModel
{
    public override BotanistElement Element => BotanistElement.Fire;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(8m, ValueProp.Move),
        new DynamicVar("BonusDamage", 4m)
    ];

    protected override bool ShouldGlowGoldInternal => LastPlayedCardWasSeedOrCutting();

    public BotanistBlazingBud()
        : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);

        if (!LastPlayedCardWasSeedOrCutting())
        {
            return;
        }

        await DamageCmd.Attack(DynamicVars["BonusDamage"].BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["BonusDamage"].UpgradeValueBy(2m);
    }

    private bool LastPlayedCardWasSeedOrCutting()
    {
        CardPlayFinishedEntry? previous = CombatManager.Instance?.History.CardPlaysFinished
            .LastOrDefault(entry =>
                entry.CardPlay.Card.Owner == Owner &&
                !ReferenceEquals(entry.CardPlay.Card, this));
        return previous?.CardPlay.Card is { } card && (card.IsSeed() || card.IsCutting());
    }
}
