// 中文卡名：以太
// 卡面描述：培养区中所有种子的每种所需元素计数各减少1。
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace Botanist.Scripts;

[Pool(typeof(TokenCardPool))]
public class BotanistAether : BotanistCardModel
{
    public override bool CanBeGeneratedInCombat => false;
    public override BotanistElement Element => BotanistElement.Aether;
    public override string PortraitPath => "res://botanist/images/botanist_character.svg";

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];

    public BotanistAether() : base(1, CardType.Skill, CardRarity.Token, TargetType.Self, true)
    {
    }

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        return Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
