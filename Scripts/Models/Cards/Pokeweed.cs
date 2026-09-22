// 中文卡名：章柳根
// 卡面描述：
// 造成{Damage:diff()}点伤害。
// [gold]成长[/gold]：随机给予敌人{PoisonPower:diff()}层[gold]中毒[/gold]{Repeat:diff()}次。
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
public class BotanistPokeweed : BotanistTargetedSeedCardModel
{
    public override BotanistElement Element => BotanistElement.Water;
    public override string PortraitPath => BotanistArt.Pokeweed;

    public override string RipenSummary =>
        $"随机给予敌人{DynamicVars.Poison.IntValue}层中毒{DynamicVars.Repeat.IntValue}次";

    public override IReadOnlyList<KeyValuePair<BotanistElement, int>> Requirements =>
    [
        new(BotanistElement.Water, 1),
        new(BotanistElement.Earth, 2)
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(6m, ValueProp.Move),
        new PowerVar<PoisonPower>(3m),
        new RepeatVar(3)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<PoisonPower>()];

    public BotanistPokeweed()
        : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, true)
    {
    }

    protected override async Task OnSow(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(SowTarget)
            .Execute(choiceContext);
    }

    public override async Task OnRipen(PlayerChoiceContext choiceContext)
    {
        if (RipenCombatState is not { } combatState)
        {
            return;
        }

        for (int i = 0; i < DynamicVars.Repeat.IntValue; i++)
        {
            var target = Owner.RunState.Rng.CombatTargets.NextItem(combatState.HittableEnemies);
            if (target == null)
            {
                continue;
            }

            await PowerCmd.Apply<PoisonPower>(
                choiceContext,
                target,
                DynamicVars.Poison.BaseValue,
                Owner.Creature,
                this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Repeat.UpgradeValueBy(1m);
    }
}
