// 中文卡名：紫颂花
// 卡面描述：
// 只有在你的培育区中没有[gold]种子[/gold]时才能打出。
// [gold]成长[/gold]：将等于你培育区中[gold]种子[/gold]数{IfUpgraded:show:+1|}的[gold]紫颂果[/gold]加入你的手牌。
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace Botanist.Scripts;

[Pool(typeof(BotanistCardPool))]
public class BotanistChorusFlower : BotanistSeedCardModel
{
    public override BotanistElement Element => BotanistElement.Wind;
    public override string PortraitPath => BotanistArt.ChorusFlower;

    public override string RipenSummary =>
        $"将等于培育区中种子数{(IsUpgraded ? "+1" : string.Empty)}的紫颂果加入手牌";

    public override IReadOnlyList<KeyValuePair<BotanistElement, int>> Requirements =>
    [
        new(BotanistElement.Fire, 2),
        new(BotanistElement.Water, 2),
        new(BotanistElement.Wind, 2)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromCard<BotanistChorusFruit>()];

    protected override bool IsPlayable =>
        BotanistCultivation.GetPlanted(Owner).Count == 0;

    protected override bool ShouldGlowGoldInternal => IsPlayable;

    public BotanistChorusFlower() : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self, true)
    {
    }

    public override async Task OnRipen(PlayerChoiceContext choiceContext)
    {
        if (CombatState == null)
        {
            return;
        }

        // 成熟效果结算时这张牌尚未离场，因此培育区数量要包含自身。
        int fruitCount = BotanistCultivation.GetPlanted(Owner).Count + 1;
        if (IsUpgraded)
        {
            fruitCount++;
        }

        List<CardModel> fruits = [];
        for (int i = 0; i < fruitCount; i++)
        {
            fruits.Add(CombatState.CreateCard<BotanistChorusFruit>(Owner));
        }

        IReadOnlyList<CardPileAddResult> results = await CardPileCmd.AddGeneratedCardsToCombat(
            fruits,
            PileType.Hand,
            Owner);
        CardCmd.PreviewCardPileAdd(results, 2.2f);
    }
}
