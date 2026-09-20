// 中文卡名：温室效应
// 卡面描述：每当你打出一张耗能大于等于2的牌时，吸取一次[gold]火元素[/gold]。
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Botanist.Scripts;

[Pool(typeof(BotanistCardPool))]
public class BotanistGreenhouseEffect : BotanistCardModel
{
    public override BotanistElement Element => BotanistElement.Fire;

    public BotanistGreenhouseEffect()
        : base(2, CardType.Power, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<BotanistGreenhouseEffectPower>(
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
