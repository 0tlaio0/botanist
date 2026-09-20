// 中文卡名：堆肥
// 卡面描述：{IfUpgraded:show:选择消耗1张手牌|随机消耗1张手牌}，使培养区中最后一颗[gold]种子[/gold]所有元素计数减少1。
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Botanist.Scripts;

[Pool(typeof(BotanistCardPool))]
public class BotanistCompost : BotanistCardModel
{
    public override BotanistElement Element => BotanistElement.Earth;

    public BotanistCompost() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        CardModel? consumedCard = IsUpgraded
            ? (await CardSelectCmd.FromHand(
                choiceContext,
                Owner,
                new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, 1),
                filter: null,
                source: this)).FirstOrDefault()
            : Owner.RunState.Rng.CombatCardSelection.NextItem(PileType.Hand.GetPile(Owner).Cards);

        if (consumedCard == null)
        {
            return;
        }

        await CardCmd.Exhaust(choiceContext, consumedCard);
        await BotanistCultivation.ReduceLastSeedGrowth(choiceContext, Owner);
    }
}
