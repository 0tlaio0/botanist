// 中文卡名：缠结倒刺
// 卡面描述：
// 给予敌人{Binding:diff()}回合[gold]禁锢[/gold]。
// 给予所有敌人{VulnerablePower:diff()}层[gold]易伤[/gold]。
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
public class BotanistTangledThorns : BotanistCardModel
{
    private const string BindingKey = "Binding";

    public override BotanistElement Element => BotanistElement.Earth;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<BotanistBindingPower>(BindingKey, 2m),
        new PowerVar<VulnerablePower>(2m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<BotanistBindingPower>(),
        HoverTipFactory.FromPower<VulnerablePower>()
    ];

    public BotanistTangledThorns() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target is { } target)
        {
            await PowerCmd.Apply<BotanistBindingPower>(
                choiceContext,
                target,
                DynamicVars[BindingKey].BaseValue,
                Owner.Creature,
                this);
        }

        if (CombatState is { } combatState)
        {
            await PowerCmd.Apply<VulnerablePower>(
                choiceContext,
                combatState.HittableEnemies,
                DynamicVars.Vulnerable.BaseValue,
                Owner.Creature,
                this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars[BindingKey].UpgradeValueBy(1m);
    }
}
