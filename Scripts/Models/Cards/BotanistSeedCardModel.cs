using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Botanist.Scripts;

public interface IBotanistSeedCard
{
    CardModel Card { get; }
    IReadOnlyList<KeyValuePair<BotanistElement, int>> Requirements { get; }
    string RipenSummary { get; }
    bool StealsPreviousSeedGrowth { get; }
    Task OnRipen(PlayerChoiceContext choiceContext);
    Task MoveToResultPileAfterCultivation();
}

public static class BotanistSeedCardExtensions
{
    public static bool IsSeed(this CardModel card) =>
        card.Tags.Contains(BotanistCardTags.Seed) &&
        card.Keywords.Contains(BotanistKeywords.Growth);

    public static IBotanistSeedCard? AsSeed(this CardModel card) =>
        card.IsSeed() ? card as IBotanistSeedCard : null;
}

/// <summary>
/// 提供种子共用的培育行为。种子是标签而不是卡牌类型，
/// 每张具体卡牌都必须向基类构造函数传入自己的真实卡牌类型。
/// </summary>
public abstract class BotanistSeedCardModel : BotanistCardModel, IBotanistSeedCard
{
    public CardModel Card => this;
    protected ICombatState? RipenCombatState =>
        CombatState ?? Owner?.Creature.CombatState;

    public abstract IReadOnlyList<KeyValuePair<BotanistElement, int>> Requirements { get; }
    public abstract string RipenSummary { get; }
    public virtual bool StealsPreviousSeedGrowth => false;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        BotanistKeywords.Growth
    ];

    protected override HashSet<CardTag> CanonicalTags =>
        [BotanistCardTags.Seed];

    protected BotanistSeedCardModel(
        int energyCost,
        CardType cardType,
        CardRarity rarity,
        TargetType targetType,
        bool shouldShowInCardLibrary)
        : base(energyCost, cardType, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    // 种子原卡要留在战斗内承载培育状态，不能被打牌流程立即送入弃牌堆。
    protected override PileType GetResultPileTypeForCardPlay() => PileType.Play;

    public abstract Task OnRipen(PlayerChoiceContext choiceContext);

    public async Task MoveToResultPileAfterCultivation()
    {
        if (Pile?.Type != PileType.Play)
        {
            return;
        }

        await CardPileCmd.Add(this, PileType.Discard);
    }
}
