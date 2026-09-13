// 中文卡名：标本
// 卡面描述：
// 预览：复制生成标本的种子成长效果。
// 生成后：{Growth}。
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace Botanist.Scripts;

[Pool(typeof(TokenCardPool))]
public class BotanistSpecimen : BotanistCardModel
{
    private IBotanistSeedCard? _copiedSeed;

    public override bool CanBeGeneratedInCombat => false;
    public override BotanistElement Element => BotanistElement.Wind;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new StringVar("Growth", "无效果")];

    public BotanistSpecimen() : base(1, CardType.Skill, CardRarity.Token, TargetType.Self, true)
    {
    }

    public void SetCopiedSeed(IBotanistSeedCard? seed)
    {
        _copiedSeed = seed;
        ((StringVar)DynamicVars["Growth"]).StringValue = GrowthText;
    }

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        return _copiedSeed == null
            ? Task.CompletedTask
            : BotanistGrowthResolution.ResolveAsync(_copiedSeed, choiceContext);
    }

    protected override void AddExtraArgsToDescription(LocString description)
    {
        description.Add("HasCopiedSeed", _copiedSeed != null);
    }

    private string GrowthText => _copiedSeed?.RipenSummary ?? "无效果";
}
