// 中文卡名：泰坦魔芋
// 卡面描述：
// [gold]成长[/gold]：使所有敌人的[gold]中毒[/gold]层数翻倍。
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Botanist.Scripts;

[Pool(typeof(BotanistCardPool))]
public class BotanistTitanArum : BotanistSeedCardModel
{
    public override BotanistElement Element => BotanistElement.Earth;

    public override string RipenSummary => "使所有敌人的中毒层数翻倍";

    public override IReadOnlyList<KeyValuePair<BotanistElement, int>> Requirements =>
    [
        new(BotanistElement.Earth, 2),
        new(BotanistElement.Water, 2)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        BotanistKeywords.Growth,
        CardKeyword.Exhaust
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<PoisonPower>()];

    public BotanistTitanArum()
        : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    public override async Task OnRipen(PlayerChoiceContext choiceContext)
    {
        if (RipenCombatState is not { } combatState)
        {
            return;
        }

        foreach (var enemy in combatState.HittableEnemies)
        {
            if (enemy.GetPower<PoisonPower>() is not { Amount: > 0 } poison)
            {
                continue;
            }

            // 施加者为空，避免“植物毒理学”把翻倍视为新给予的中毒。
            await PowerCmd.ModifyAmount(
                choiceContext,
                poison,
                poison.Amount,
                null,
                this);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
