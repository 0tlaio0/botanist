using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Qgs.Scripts;

public interface IQgsSeedCard
{
    CardModel Card { get; }
    IReadOnlyList<KeyValuePair<QgsElement, int>> Requirements { get; }
    string RipenSummary { get; }
    Task OnRipen(PlayerChoiceContext choiceContext);
}

public static class QgsSeedCardExtensions
{
    public static bool IsSeed(this CardModel card) =>
        card.Tags.Contains(QgsCardTags.Seed) &&
        card.Keywords.Contains(QgsKeywords.Growth);

    public static IQgsSeedCard? AsSeed(this CardModel card) =>
        card.IsSeed() ? card as IQgsSeedCard : null;
}

/// <summary>
/// 提供种子共用的培育行为。种子是标签而不是卡牌类型，
/// 每张具体卡牌都必须向基类构造函数传入自己的真实卡牌类型。
/// </summary>
public abstract class QgsSeedCardModel : QgsCardModel, IQgsSeedCard
{
    public CardModel Card => this;
    public abstract IReadOnlyList<KeyValuePair<QgsElement, int>> Requirements { get; }
    public abstract string RipenSummary { get; }

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        QgsKeywords.Growth
    ];

    protected override HashSet<CardTag> CanonicalTags =>
        [QgsCardTags.Seed];

    protected QgsSeedCardModel(
        int energyCost,
        CardType cardType,
        CardRarity rarity,
        TargetType targetType,
        bool shouldShowInCardLibrary)
        : base(energyCost, cardType, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    // 打出后先保留在标准 PlayPile，待完整结算后再决定进入培养区或弃牌堆。
    protected override PileType GetResultPileTypeForCardPlay() => PileType.Play;

    public abstract Task OnRipen(PlayerChoiceContext choiceContext);
}
