// 中文卡名：季风
// 卡面描述：抽{IfUpgraded:show:X+1|X}张牌，吸取{IfUpgraded:show:X+1|X}次[gold]风元素[/gold]。
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Botanist.Scripts;

[Pool(typeof(BotanistCardPool))]
public class BotanistMonsoon : BotanistCardModel
{
    public override BotanistElement Element => BotanistElement.Wind;
    protected override bool HasEnergyCostX => true;

    public BotanistMonsoon() : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int amount = ResolveEnergyXValue() + (IsUpgraded ? 1 : 0);
        if (amount > 0)
        {
            await CardPileCmd.Draw(choiceContext, amount, Owner);
        }

        for (int i = 0; i < amount; i++)
        {
            await BotanistCultivation.AbsorbElement(choiceContext, Owner, BotanistElement.Wind);
        }
    }

    protected override void OnUpgrade()
    {
    }
}
