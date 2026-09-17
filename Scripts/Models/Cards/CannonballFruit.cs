// 中文卡名：陨炮
// 卡面描述：[gold]成长[/gold]：对所有敌人造成{Damage:diff()}点伤害。
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
public class BotanistCannonballFruit : BotanistSeedCardModel
{
    public override BotanistElement Element => BotanistElement.Fire;
    public override string PortraitPath => BotanistArt.CannonballFruit;

    public override string RipenSummary =>
        $"对所有敌人造成{DynamicVars.Damage.IntValue}点伤害";

    public override IReadOnlyList<KeyValuePair<BotanistElement, int>> Requirements =>
        [new(BotanistElement.Fire, 2)];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(12, ValueProp.Move)];

    public BotanistCannonballFruit()
        : base(1, CardType.Attack, CardRarity.Common, TargetType.AllEnemies, true)
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
            .TargetingAllOpponents(combatState)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4);
    }
}
