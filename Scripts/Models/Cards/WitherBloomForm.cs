// 中文卡名：枯荣形态
// 卡面描述：
// 当[gold]种子[/gold]进入培养区时，使其前一颗[gold]种子[/gold]每种元素所需计数减少1。
// 若本回合没有[gold]种子[/gold][gold]成长[/gold]，失去{HpLoss:diff()}点生命。
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Botanist.Scripts;

[Pool(typeof(BotanistCardPool))]
public class BotanistWitherBloomForm : BotanistCardModel
{
    public override BotanistElement Element => BotanistElement.Aether;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar("HpLoss", 10m, DamageProps.nonCardHpLoss)];

    public BotanistWitherBloomForm()
        : base(3, CardType.Power, CardRarity.Rare, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        BotanistWitherBloomFormPower power =
            (BotanistWitherBloomFormPower)ModelDb.Power<BotanistWitherBloomFormPower>().ToMutable();
        power.SetHpLoss(DynamicVars["HpLoss"].BaseValue);

        await PowerCmd.Apply(
            choiceContext,
            power,
            Owner.Creature,
            1m,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["HpLoss"].UpgradeValueBy(-4m);
    }
}
