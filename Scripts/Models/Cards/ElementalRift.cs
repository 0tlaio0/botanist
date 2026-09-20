// 中文卡名：原素裂隙
// 卡面描述：将1张{IfUpgraded:show:[gold]以太+[/gold]|[gold]以太[/gold]}加入你的手牌。
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace Botanist.Scripts;

[Pool(typeof(BotanistCardPool))]
public class BotanistElementalRift : BotanistCardModel
{
    public override BotanistElement Element => BotanistElement.None;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromCard<BotanistAether>(IsUpgraded)];

    public BotanistElementalRift()
        : base(0, CardType.Skill, CardRarity.Common, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner.Creature.CombatState is not { } combatState)
        {
            return;
        }

        BotanistAether aether = combatState.CreateCard<BotanistAether>(Owner);
        if (IsUpgraded)
        {
            CardCmd.Upgrade(aether, CardPreviewStyle.None);
        }

        await CardPileCmd.AddGeneratedCardToCombat(aether, PileType.Hand, Owner);
    }

    protected override void OnUpgrade()
    {
    }
}
