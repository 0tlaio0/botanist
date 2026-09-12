// 中文卡名：标本夹
// 卡面描述：打出此牌后，你在这个回合内每成长一颗[gold]种子[/gold]，将一张[gold]标本[/gold]加入你的抽牌堆。
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace Botanist.Scripts;

[Pool(typeof(BotanistCardPool))]
public class BotanistFieldSpecimenCase : BotanistCardModel
{
    public override BotanistElement Element => BotanistElement.Fire;
    public override string PortraitPath => "res://botanist/images/botanist_character.svg";

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromCard<BotanistSpecimen>()];

    public BotanistFieldSpecimenCase() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<BotanistSpecimenCasePower>(
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
