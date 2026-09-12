// 中文卡名：种质库
// 卡面描述：
// 种子在第一次成长后，在本场战斗的耗能减少1。
// 本回合此前每成长一颗种子，这张卡的费用减少1。
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Botanist.Scripts;

[Pool(typeof(BotanistCardPool))]
public class BotanistSeedBank : BotanistCardModel
{
    public override BotanistElement Element => BotanistElement.Earth;

    public BotanistSeedBank() : base(5, CardType.Power, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    public static void RefreshInHand(Player player)
    {
        foreach (CardModel card in PileType.Hand.GetPile(player).Cards)
        {
            if (card is BotanistSeedBank)
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

        int seedsCultivated = BotanistCultivation.GetSeedsCultivatedThisTurn(Owner);
        if (seedsCultivated <= 0)
        {
            return false;
        }

        modifiedCost = Math.Max(0m, originalCost - seedsCultivated);
        return modifiedCost != originalCost;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<BotanistSeedBankPower>(
            choiceContext,
            Owner.Creature,
            1m,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}
