using System;
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

namespace Qgs.Scripts;

// 战斗奖励会排除基础稀有度。没有普通/罕见/稀有牌时，胜利结算会在生成卡牌奖励时中断，界面也就没有继续选项。
[Pool(typeof(QgsCardPool))]
public class QgsPruner : QgsCardModel
{
    public override QgsElement Element => QgsElement.Fire;
    public override string PortraitPath => "res://qgs/images/qgs_character.svg";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(9, ValueProp.Move)];

    public QgsPruner() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
    }
}

[Pool(typeof(QgsCardPool))]
public class QgsSweepNet : QgsCardModel
{
    public override QgsElement Element => QgsElement.Wind;
    public override string PortraitPath => "res://qgs/images/qgs_character.svg";

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<WeakPower>()];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(6, ValueProp.Move),
        new PowerVar<WeakPower>(1m)
    ];

    public QgsSweepNet() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
        await PowerCmd.Apply<WeakPower>(choiceContext, cardPlay.Target, DynamicVars.Weak.BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
        DynamicVars.Weak.UpgradeValueBy(1);
    }
}

[Pool(typeof(QgsCardPool))]
public class QgsTrowel : QgsCardModel
{
    public override QgsElement Element => QgsElement.Earth;
    public override bool GainsBlock => true;
    public override string PortraitPath => "res://qgs/images/qgs_character.svg";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new BlockVar(8, ValueProp.Move)];

    public QgsTrowel() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3);
    }
}

[Pool(typeof(QgsCardPool))]
public class QgsWatering : QgsCardModel
{
    public override QgsElement Element => QgsElement.Water;
    public override string PortraitPath => "res://qgs/images/qgs_character.svg";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new CardsVar(2)];

    public QgsWatering() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1);
    }
}

[Pool(typeof(QgsCardPool))]
public class QgsGloves : QgsCardModel
{
    public override QgsElement Element => QgsElement.Earth;
    public override string PortraitPath => "res://qgs/images/qgs_character.svg";

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<DexterityPower>()];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new PowerVar<DexterityPower>(2m)];

    public QgsGloves() : base(1, CardType.Power, CardRarity.Common, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<DexterityPower>(choiceContext, Owner.Creature, DynamicVars.Dexterity.BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Dexterity.UpgradeValueBy(1);
    }
}

[Pool(typeof(QgsCardPool))]
public class QgsCompost : QgsCardModel
{
    public override QgsElement Element => QgsElement.Fire;
    public override string PortraitPath => "res://qgs/images/qgs_character.svg";

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<StrengthPower>()];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new PowerVar<StrengthPower>(2m)];

    public QgsCompost() : base(1, CardType.Power, CardRarity.Common, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, DynamicVars.Strength.BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Strength.UpgradeValueBy(1);
    }
}

[Pool(typeof(QgsCardPool))]
public class QgsSoilExcavation : QgsCardModel
{
    public override QgsElement Element => QgsElement.Earth;
    public override string PortraitPath => "res://qgs/images/qgs_character.svg";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new IntVar("Capacity", 2m)];

    public QgsSoilExcavation() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        QgsCultivation.AddCapacity(Owner, DynamicVars["Capacity"].IntValue);
        return Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Capacity"].UpgradeValueBy(1);
    }
}
