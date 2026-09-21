// 中文卡名：插条
// 卡面描述：造成{Damage:diff()}点伤害。使培养区中最后一颗[gold]种子[/gold]的该元素计数再减少1。
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace Botanist.Scripts;

[Pool(typeof(TokenCardPool))]
public class BotanistCutting : BotanistCardModel
{
    private BotanistElement _element = BotanistElement.Wind;

    public override bool CanBeGeneratedInCombat => false;
    public override BotanistElement Element => _element;
    public bool ForksToLowest { get; set; }

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [CardKeyword.Exhaust];

    protected override HashSet<CardTag> CanonicalTags =>
        [BotanistCardTags.Cutting];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(3m, ValueProp.Move)];

    public BotanistCutting()
        : base(0, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy, true)
    {
    }

    public void SetElement(BotanistElement element)
    {
        _element = element;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);

        if (ForksToLowest && CombatState is { } combatState)
        {
            Creature? lowest = combatState.HittableEnemies
                .OrderBy(enemy => enemy.CurrentHp)
                .FirstOrDefault();
            if (lowest != null)
            {
                await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                    .FromCard(this)
                    .Targeting(lowest)
                    .Execute(choiceContext);
            }
        }

        BotanistCultivation.RecordPlayedCuttingElement(Owner, Element);
        BotanistCultivation.RecordCuttingPlayedThisTurn(Owner);
        await BotanistCultivation.ReduceLastSeedElement(choiceContext, Owner, Element);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
    }
}
