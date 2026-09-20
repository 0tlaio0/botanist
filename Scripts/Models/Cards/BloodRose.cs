// 中文卡名：血蔷薇
// 卡面描述：
// 回合开始时若这张牌在培育区，失去{HpLoss:diff()}点生命。
// [gold]成长[/gold]：对生命值最高的敌人造成{Damage:diff()}点伤害，回复造成伤害一半的生命值。
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Botanist.Scripts;

[Pool(typeof(BotanistCardPool))]
public class BotanistBloodRose : BotanistSeedCardModel
{
    public override BotanistElement Element => BotanistElement.Water;

    public override string RipenSummary =>
        $"对生命值最高的敌人造成{DynamicVars.Damage.IntValue}点伤害，回复造成伤害一半的生命值";

    public override IReadOnlyList<KeyValuePair<BotanistElement, int>> Requirements =>
    [
        new(BotanistElement.Fire, 2),
        new(BotanistElement.Water, 2),
        new(BotanistElement.Earth, 2)
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(28m, ValueProp.Move),
        new DamageVar("HpLoss", 4m, DamageProps.cardHpLoss)
    ];

    public BotanistBloodRose()
        : base(2, CardType.Attack, CardRarity.Rare, TargetType.RandomEnemy, true)
    {
    }

    public override async Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext,
        MegaCrit.Sts2.Core.Entities.Players.Player player)
    {
        if (player != Owner ||
            Owner.Creature.IsDead ||
            !BotanistCultivation.GetPlanted(player).Any(seed => seed.Card == this))
        {
            return;
        }

        await CreatureCmd.Damage(
            choiceContext,
            Owner.Creature,
            DynamicVars["HpLoss"].BaseValue,
            DamageProps.cardHpLoss,
            null,
            this);
    }

    public override async Task OnRipen(PlayerChoiceContext choiceContext)
    {
        if (RipenCombatState is not { } combatState)
        {
            return;
        }

        Creature? target = combatState.HittableEnemies
            .OrderByDescending(enemy => enemy.CurrentHp)
            .FirstOrDefault();
        if (target == null)
        {
            return;
        }

        var attack = await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(target)
            .Execute(choiceContext);

        int healing = attack.Results
            .SelectMany(hit => hit)
            .Sum(result => result.UnblockedDamage) / 2;
        if (healing > 0)
        {
            await CreatureCmd.Heal(Owner.Creature, healing);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(8m);
    }
}
