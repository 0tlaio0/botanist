// 中文卡名：世界树枝条
// 卡面描述：将{Aethers:diff()}张[gold]以太[/gold]加入你的抽牌堆。
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Botanist.Scripts;

[Pool(typeof(BotanistCardPool))]
public class BotanistWorldTreeBranch : BotanistCardModel
{
    public override BotanistElement Element => BotanistElement.Wind;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromCard<BotanistAether>()];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new IntVar("Aethers", 3m)];

    public BotanistWorldTreeBranch() : base(3, CardType.Skill, CardRarity.Ancient, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var combatState = CombatState;
        if (combatState == null)
        {
            return;
        }

        List<CardModel> aethers = [];
        for (int i = 0; i < DynamicVars["Aethers"].IntValue; i++)
        {
            aethers.Add(combatState.CreateCard<BotanistAether>(Owner));
        }

        await CardPileCmd.AddGeneratedCardsToCombat(
            aethers,
            PileType.Draw,
            Owner,
            CardPilePosition.Random);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
