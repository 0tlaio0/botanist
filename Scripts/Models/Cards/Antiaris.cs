// 中文卡名：见血封喉
// 卡面描述：
// [gold]成长[/gold]：对所有敌人造成{Damage:diff()}点伤害{Repeat:diff()}次，每次造成未被格挡的伤害时给予{PoisonPower:diff()}层[gold]中毒[/gold]。
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
public class BotanistAntiaris : BotanistSeedCardModel
{
    public override BotanistElement Element => BotanistElement.Fire;
    public override string PortraitPath => BotanistArt.Antiaris;

    public override string RipenSummary =>
        $"对所有敌人造成{DynamicVars.Damage.IntValue}点伤害{DynamicVars.Repeat.IntValue}次，" +
        $"每次造成未被格挡的伤害时给予{DynamicVars.Poison.IntValue}层中毒";

    public override IReadOnlyList<KeyValuePair<BotanistElement, int>> Requirements =>
    [
        new(BotanistElement.Fire, 1),
        new(BotanistElement.Water, 1),
        new(BotanistElement.Earth, 1)
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(2m, ValueProp.Move),
        new PowerVar<PoisonPower>(3m),
        new RepeatVar(3)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<PoisonPower>()];

    public BotanistAntiaris()
        : base(1, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies, true)
    {
    }

    public override async Task OnRipen(PlayerChoiceContext choiceContext)
    {
        if (CombatState is not { } combatState)
        {
            return;
        }

        var attack = await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .TargetingAllOpponents(combatState)
            .WithHitCount(DynamicVars.Repeat.IntValue)
            .Execute(choiceContext);

        foreach (var hit in attack.Results)
        {
            foreach (var result in hit)
            {
                if (result.UnblockedDamage <= 0 || !result.Receiver.IsAlive)
                {
                    continue;
                }

                await PowerCmd.Apply<PoisonPower>(
                    choiceContext,
                    result.Receiver,
                    DynamicVars.Poison.BaseValue,
                    Owner.Creature,
                    this);
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Repeat.UpgradeValueBy(1m);
    }
}
