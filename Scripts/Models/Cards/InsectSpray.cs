// 中文卡名：除虫喷雾
// 卡面描述：给予所有敌人{PoisonPower:diff()}层[gold]中毒[/gold]。
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
public class BotanistInsectSpray : BotanistCardModel
{
    public override BotanistElement Element => BotanistElement.Water;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new PowerVar<PoisonPower>(5m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<PoisonPower>()];

    public BotanistInsectSpray()
        : base(1, CardType.Skill, CardRarity.Common, TargetType.AllEnemies, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (CombatState is not { } combatState)
        {
            return;
        }

        await PowerCmd.Apply<PoisonPower>(
            choiceContext,
            combatState.HittableEnemies,
            DynamicVars.Poison.BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Poison.UpgradeValueBy(2m);
    }
}