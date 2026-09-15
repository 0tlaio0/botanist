// 中文卡名：缓释肥
// 卡面描述：令下一颗[gold]种子[/gold]成长需求减少1。
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Botanist.Scripts;

[Pool(typeof(BotanistCardPool))]
public class BotanistSlowReleaseFertilizer : BotanistCardModel
{
    public override BotanistElement Element => BotanistElement.Earth;

    public BotanistSlowReleaseFertilizer()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<BotanistSlowReleaseFertilizerPower>(
            choiceContext,
            Owner.Creature,
            1m,
            Owner.Creature,
            this);
        BotanistCardChrome.RefreshSeedPreviews(Owner);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}
