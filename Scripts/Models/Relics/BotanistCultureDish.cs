using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Botanist.Scripts;

/// <summary>每次打出嫁接牌时，使手牌中随机一张牌本回合免费。</summary>
[Pool(typeof(BotanistRelicPool))]
public class BotanistCultureDish : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    public override string PackedIconPath => BotanistArt.Character;
    protected override string PackedIconOutlinePath => BotanistArt.Character;
    protected override string BigIconPath => BotanistArt.Character;

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!CombatManager.Instance.IsInProgress ||
            cardPlay.Card.Owner != Owner ||
            BotanistGraftService.GetGraftSnapshots(cardPlay.Card).Count == 0)
        {
            return Task.CompletedTask;
        }

        CardModel? card = Owner.RunState.Rng.CombatCardSelection.NextItem(
            PileType.Hand.GetPile(Owner).Cards);
        if (card == null)
        {
            return Task.CompletedTask;
        }

        card.SetToFreeThisTurn();
        Flash();
        return Task.CompletedTask;
    }
}
