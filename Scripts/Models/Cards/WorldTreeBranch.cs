// 中文卡名：世界树枝条
// 卡面描述：在你的回合开始时，将1张[gold]以太[/gold]加入你的手牌。
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace Botanist.Scripts;

[Pool(typeof(BotanistCardPool))]
public class BotanistWorldTreeBranch : BotanistCardModel
{
    public override BotanistElement Element => BotanistElement.Aether;
    public override string PortraitPath => BotanistArt.WorldTreeBranch;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromCard<BotanistAether>()];

    public BotanistWorldTreeBranch() : base(3, CardType.Power, CardRarity.Ancient, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<BotanistWorldTreeBranchPower>(
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
