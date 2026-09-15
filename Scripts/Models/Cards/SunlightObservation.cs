// 中文卡名：日照观测
// 卡面描述：
// 获得1层[gold]向阳[/gold]。
// 本回合每吸取一次[gold]火元素[/gold]，这张卡的费用减少1。
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace Botanist.Scripts;

[Pool(typeof(BotanistCardPool))]
public class BotanistSunlightObservation : BotanistCardModel
{
    public override BotanistElement Element => BotanistElement.Fire;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<BotanistSunwardPower>()];

    public BotanistSunlightObservation()
        : base(2, CardType.Power, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    public static void RefreshInHand(Player player)
    {
        foreach (CardModel card in PileType.Hand.GetPile(player).Cards)
        {
            if (card is BotanistSunlightObservation)
            {
                card.InvokeEnergyCostChanged();
            }
        }
    }

    public override bool TryModifyEnergyCostInCombat(
        CardModel card,
        decimal originalCost,
        out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (card != this || Owner == null)
        {
            return false;
        }

        int fireAbsorbed = BotanistCultivation.GetFireAbsorbedThisTurn(Owner);
        if (fireAbsorbed <= 0)
        {
            return false;
        }

        modifiedCost = Math.Max(0m, originalCost - fireAbsorbed);
        return modifiedCost != originalCost;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<BotanistSunwardPower>(
            choiceContext,
            Owner.Creature,
            1m,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
